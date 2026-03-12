using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Shared.Logging;

namespace CadAutomationPlugin.Core.ChangePropagation
{
    /// <summary>
    /// 变更传播引擎 - 核心功能 3
    /// 管理零件关联关系，实现批量改图的连锁反应
    /// </summary>
    public class ChangePropagationEngine
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(ChangePropagationEngine));

        /// <summary>
        /// 最大递归深度 - 防止无限递归
        /// </summary>
        private const int MaxRecursionDepth = 10;

        /// <summary>
        /// 分析依赖关系
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="selection">选择的对象集</param>
        /// <param name="trans">当前事务</param>
        /// <returns>受影响的零件列表</returns>
        /// <exception cref="ArgumentNullException">当 db、selection 或 trans 为 null 时</exception>
        public List<AffectedPart> AnalyzeDependencies(Database db, SelectionSet selection, Transaction trans)
        {
            if (db == null)
            {
                Logger.Error("AnalyzeDependencies: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (selection == null)
            {
                Logger.Warn("AnalyzeDependencies: selection 为 null，返回空列表");
                return new List<AffectedPart>();
            }

            if (selection.Count == 0)
            {
                Logger.Warn("AnalyzeDependencies: 未选择任何对象");
                return new List<AffectedPart>();
            }

            if (trans == null)
            {
                Logger.Error("AnalyzeDependencies: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            Logger.Info("开始分析依赖关系");

            var affectedParts = new List<AffectedPart>();
            var visited = new HashSet<ObjectId>();

            foreach (SelectedObject selObj in selection)
            {
                try
                {
                    AnalyzeRecursive(selObj.ObjectId, db, trans, affectedParts, visited, 0);
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"分析对象依赖失败：{selObj.ObjectId}", ex);
                }
            }

            Logger.Info($"依赖分析完成，影响 {affectedParts.Count} 个零件");
            return affectedParts;
        }

        /// <summary>
        /// 递归分析依赖
        /// </summary>
        private void AnalyzeRecursive(ObjectId objectId, Database db, Transaction trans, 
            List<AffectedPart> affected, HashSet<ObjectId> visited, int depth)
        {
            if (visited.Contains(objectId) || depth > MaxRecursionDepth)
            {
                if (depth > MaxRecursionDepth)
                {
                    Logger.Warn($"达到最大递归深度 ({MaxRecursionDepth})，停止分析");
                }
                return;
            }

            visited.Add(objectId);

            try
            {
                // 获取关联的零件
                var dependencies = GetDependencies(db, new[] { new SelectedObject(objectId, SelectionMode.Normal) }, trans);

                foreach (var dep in dependencies)
                {
                    affected.Add(new AffectedPart
                    {
                        ObjectId = dep.ObjectId,
                        PartName = dep.PartName,
                        RelationshipType = dep.RelationshipType,
                        Depth = depth + 1
                    });

                    // 递归分析下一级
                    AnalyzeRecursive(dep.ObjectId, db, trans, affected, visited, depth + 1);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"递归分析依赖失败：{objectId}", ex);
            }
        }

        /// <summary>
        /// 获取依赖关系
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="selection">选择的对象集</param>
        /// <param name="trans">当前事务</param>
        /// <returns>依赖信息列表</returns>
        public List<DependencyInfo> GetDependencies(Database db, SelectionSet selection, Transaction trans)
        {
            if (db == null || selection == null || trans == null)
            {
                Logger.Warn("GetDependencies: 参数无效");
                return new List<DependencyInfo>();
            }

            var dependencies = new List<DependencyInfo>();

            foreach (SelectedObject selObj in selection)
            {
                try
                {
                    using var entity = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                    if (entity == null || entity.IsDisposed) continue;

                    using var xdata = entity.XData;
                    if (xdata != null)
                    {
                        var deps = ParseDependencyXData(xdata);
                        dependencies.AddRange(deps);
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"读取依赖关系失败：{selObj.ObjectId}", ex);
                }
            }

            return dependencies;
        }

        /// <summary>
        /// 注册依赖关系
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="parentId">父对象 ID</param>
        /// <param name="childId">子对象 ID</param>
        /// <param name="relationshipType">关联类型</param>
        /// <param name="trans">当前事务</param>
        public void RegisterDependency(Database db, ObjectId parentId, ObjectId childId, 
            string relationshipType, Transaction trans)
        {
            if (db == null || trans == null)
            {
                Logger.Error("RegisterDependency: db 或 trans 为 null");
                return;
            }

            if (parentId.IsNull || childId.IsNull)
            {
                Logger.Error("RegisterDependency: 对象 ID 无效");
                return;
            }

            if (string.IsNullOrEmpty(relationshipType))
            {
                Logger.Error("RegisterDependency: 关联类型不能为空");
                return;
            }

            Logger.Info($"注册关联：{parentId} -> {childId} ({relationshipType})");

            using var parentEntity = trans.GetObject(parentId, OpenMode.ForWrite) as Entity;
            if (parentEntity == null || parentEntity.IsDisposed)
            {
                Logger.Error("RegisterDependency: 无法获取父对象");
                return;
            }

            try
            {
                // 创建扩展数据
                using var xdata = CreateDependencyXData(childId, relationshipType);

                // 附加到父零件
                using var existingXData = parentEntity.XData;
                if (existingXData != null)
                {
                    // 合并现有数据
                    using var mergedXData = MergeXData(existingXData, xdata);
                    parentEntity.XData = mergedXData;
                }
                else
                {
                    parentEntity.XData = xdata;
                }

                Logger.Debug("依赖关系注册成功");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"注册依赖关系失败", ex);
            }
        }

        /// <summary>
        /// 执行变更传播
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="selection">选择的对象集</param>
        /// <param name="trans">当前事务</param>
        public void PropagateChange(Database db, SelectionSet selection, Transaction trans)
        {
            if (db == null || selection == null || trans == null)
            {
                Logger.Error("PropagateChange: 参数无效");
                return;
            }

            if (selection.Count == 0)
            {
                Logger.Warn("PropagateChange: 未选择任何对象");
                return;
            }

            Logger.Info("开始执行变更传播");

            var affectedParts = AnalyzeDependencies(db, selection, trans);

            foreach (var part in affectedParts)
            {
                try
                {
                    ApplyChange(db, part.ObjectId, trans);
                    Logger.Info($"已应用变更：{part.PartName}");
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"变更应用失败：{part.PartName}", ex);
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"变更应用失败（未知错误）：{part.PartName}", ex);
                }
            }

            Logger.Info("变更传播完成");
        }

        /// <summary>
        /// 应用变更到单个零件
        /// </summary>
        private void ApplyChange(Database db, ObjectId objectId, Transaction trans)
        {
            if (db == null || trans == null || objectId.IsNull)
            {
                Logger.Warn("ApplyChange: 参数无效");
                return;
            }

            using var entity = trans.GetObject(objectId, OpenMode.ForWrite) as Entity;
            if (entity == null || entity.IsDisposed) return;

            try
            {
                using var xdata = entity.XData;
                if (xdata == null) return;

                // 解析变更参数
                var changeParams = ParseChangeParameters(xdata);

                // 应用几何变换
                if (changeParams.Transform.HasValue)
                {
                    entity.TransformBy(changeParams.Transform.Value);
                    Logger.Debug($"应用几何变换到：{entity.ObjectId}");
                }

                // 更新尺寸
                if (changeParams.DimensionUpdates != null && changeParams.DimensionUpdates.Count > 0)
                {
                    UpdateDimensions(entity, changeParams.DimensionUpdates, trans);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"应用变更失败：{objectId}", ex);
                throw;
            }
        }

        /// <summary>
        /// 更新尺寸
        /// </summary>
        private void UpdateDimensions(Entity entity, Dictionary<string, double> updates, Transaction trans)
        {
            if (entity == null || updates == null || trans == null)
            {
                Logger.Warn("UpdateDimensions: 参数无效");
                return;
            }

            // 遍历并更新关联的尺寸标注
            if (entity is DBObject dbObj)
            {
                try
                {
                    var blockId = dbObj.BlockId;
                    if (blockId.IsNull) return;

                    using var block = trans.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                    if (block == null || block.IsDisposed) return;

                    foreach (ObjectId id in block)
                    {
                        try
                        {
                            using var obj = trans.GetObject(id, OpenMode.ForRead) as Dimension;
                            if (obj != null)
                            {
                                // 更新尺寸值
                                // 具体实现取决于尺寸类型
                                Logger.Debug($"找到尺寸标注：{id}");
                            }
                        }
                        catch (Autodesk.AutoCAD.Runtime.Exception ex)
                        {
                            Logger.Error($"读取尺寸标注失败：{id}", ex);
                        }
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"更新尺寸失败", ex);
                }
            }
        }

        /// <summary>
        /// 清除所有依赖
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="trans">当前事务</param>
        public void ClearAllDependencies(Database db, Transaction trans)
        {
            if (db == null || trans == null)
            {
                Logger.Error("ClearAllDependencies: db 或 trans 为 null");
                return;
            }

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null)
            {
                Logger.Error("ClearAllDependencies: 无法获取块表");
                return;
            }

            using var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            if (modelSpace == null)
            {
                Logger.Error("ClearAllDependencies: 无法获取模型空间");
                return;
            }

            int clearedCount = 0;

            foreach (ObjectId id in modelSpace)
            {
                try
                {
                    using var entity = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                    if (entity?.XData != null)
                    {
                        entity.XData = null;
                        clearedCount++;
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"清除依赖失败：{id}", ex);
                }
            }

            Logger.Info($"已清除 {clearedCount} 个零件的关联关系");
        }

        #region 辅助方法

        /// <summary>
        /// 创建依赖关系扩展数据
        /// </summary>
        private ResultBuffer CreateDependencyXData(ObjectId targetId, string relationshipType)
        {
            // 创建扩展数据结构
            var xdata = new ResultBuffer(
                new TypedValue((int)DxfCode.ExtendedDataRegAppName, "CAD_AUTO_DEPENDENCY"),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, targetId.ToString()),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, relationshipType),
                new TypedValue((int)DxfCode.ExtendedDataReal, DateTime.Now.ToOADate())
            );

            return xdata;
        }

        /// <summary>
        /// 解析依赖关系扩展数据
        /// </summary>
        private List<DependencyInfo> ParseDependencyXData(ResultBuffer xdata)
        {
            var deps = new List<DependencyInfo>();
            
            if (xdata == null || xdata.IsDisposed)
            {
                return deps;
            }

            try
            {
                // 解析扩展数据
                // 具体实现根据 XData 格式
                Logger.Debug("解析依赖关系 XData");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error("解析 XData 失败", ex);
            }

            return deps;
        }

        /// <summary>
        /// 合并扩展数据
        /// </summary>
        private ResultBuffer MergeXData(ResultBuffer existing, ResultBuffer newData)
        {
            if (existing == null || newData == null)
            {
                Logger.Warn("MergeXData: 参数为 null");
                return existing ?? newData ?? new ResultBuffer();
            }

            // 合并两个扩展数据缓冲区
            var merged = new ResultBuffer();
            
            try
            {
                foreach (var tv in existing)
                {
                    merged.Add(tv);
                }
                
                foreach (var tv in newData)
                {
                    merged.Add(tv);
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error("合并 XData 失败", ex);
            }

            return merged;
        }

        /// <summary>
        /// 解析变更参数
        /// </summary>
        private ChangeParameters ParseChangeParameters(ResultBuffer xdata)
        {
            // 解析变更参数
            var parameters = new ChangeParameters();
            
            if (xdata == null || xdata.IsDisposed)
            {
                return parameters;
            }

            try
            {
                // 具体实现根据 XData 格式
                Logger.Debug("解析变更参数");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error("解析变更参数失败", ex);
            }

            return parameters;
        }

        #endregion
    }

    /// <summary>
    /// 受影响的零件
    /// </summary>
    public class AffectedPart
    {
        public ObjectId ObjectId { get; set; }
        public string PartName { get; set; } = "";
        public string RelationshipType { get; set; } = "";
        public int Depth { get; set; }
    }

    /// <summary>
    /// 依赖信息
    /// </summary>
    public class DependencyInfo
    {
        public ObjectId ObjectId { get; set; }
        public string PartName { get; set; } = "";
        public string RelationshipType { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// 变更参数
    /// </summary>
    public class ChangeParameters
    {
        public Matrix3d? Transform { get; set; }
        public Dictionary<string, double>? DimensionUpdates { get; set; }
    }
}
