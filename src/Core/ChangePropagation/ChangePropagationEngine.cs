using Autodesk.AutoCAD.DatabaseServices;
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
        /// 分析依赖关系
        /// </summary>
        public List<AffectedPart> AnalyzeDependencies(Database db, SelectionSet selection, Transaction trans)
        {
            Logger.Info("开始分析依赖关系");

            var affectedParts = new List<AffectedPart>();
            var visited = new HashSet<ObjectId>();

            foreach (SelectedObject selObj in selection)
            {
                AnalyzeRecursive(selObj.ObjectId, db, trans, affectedParts, visited, 0);
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
            if (visited.Contains(objectId) || depth > 10) // 防止无限递归
                return;

            visited.Add(objectId);

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

        /// <summary>
        /// 获取依赖关系
        /// </summary>
        public List<DependencyInfo> GetDependencies(Database db, SelectionSet selection, Transaction trans)
        {
            var dependencies = new List<DependencyInfo>();

            // 从扩展数据中读取关联关系
            foreach (SelectedObject selObj in selection)
            {
                var entity = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                if (entity == null) continue;

                var xdata = entity.XData;
                if (xdata != null)
                {
                    var deps = ParseDependencyXData(xdata);
                    dependencies.AddRange(deps);
                }
            }

            return dependencies;
        }

        /// <summary>
        /// 注册依赖关系
        /// </summary>
        public void RegisterDependency(Database db, ObjectId parentId, ObjectId childId, 
            string relationshipType, Transaction trans)
        {
            Logger.Info($"注册关联：{parentId} -> {childId} ({relationshipType})");

            var parentEntity = trans.GetObject(parentId, OpenMode.ForWrite) as Entity;
            if (parentEntity == null) return;

            // 创建扩展数据
            var xdata = CreateDependencyXData(childId, relationshipType);

            // 附加到父零件
            var existingXData = parentEntity.XData;
            if (existingXData != null)
            {
                // 合并现有数据
                var mergedXData = MergeXData(existingXData, xdata);
                parentEntity.XData = mergedXData;
            }
            else
            {
                parentEntity.XData = xdata;
            }
        }

        /// <summary>
        /// 执行变更传播
        /// </summary>
        public void PropagateChange(Database db, SelectionSet selection, Transaction trans)
        {
            Logger.Info("开始执行变更传播");

            var affectedParts = AnalyzeDependencies(db, selection, trans);

            foreach (var part in affectedParts)
            {
                try
                {
                    ApplyChange(db, part.ObjectId, trans);
                    Logger.Info($"已应用变更：{part.PartName}");
                }
                catch (Exception ex)
                {
                    Logger.Error($"变更应用失败：{part.PartName}", ex);
                }
            }
        }

        /// <summary>
        /// 应用变更到单个零件
        /// </summary>
        private void ApplyChange(Database db, ObjectId objectId, Transaction trans)
        {
            var entity = trans.GetObject(objectId, OpenMode.ForWrite) as Entity;
            if (entity == null) return;

            // 根据关联类型应用相应的变更
            var xdata = entity.XData;
            if (xdata == null) return;

            // 解析变更参数
            var changeParams = ParseChangeParameters(xdata);

            // 应用几何变换
            if (changeParams.Transform.HasValue)
            {
                entity.TransformBy(changeParams.Transform.Value);
            }

            // 更新尺寸
            if (changeParams.DimensionUpdates != null)
            {
                UpdateDimensions(entity, changeParams.DimensionUpdates, trans);
            }
        }

        /// <summary>
        /// 更新尺寸
        /// </summary>
        private void UpdateDimensions(Entity entity, Dictionary<string, double> updates, Transaction trans)
        {
            // 遍历并更新关联的尺寸标注
            if (entity is DBObject dbObj)
            {
                var blockId = dbObj.BlockId;
                var block = trans.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                if (block != null)
                {
                    foreach (ObjectId id in block)
                    {
                        if (trans.GetObject(id, OpenMode.ForRead) is Dimension dim)
                        {
                            // 更新尺寸值
                            // 具体实现取决于尺寸类型
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 清除所有依赖
        /// </summary>
        public void ClearAllDependencies(Database db, Transaction trans)
        {
            var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

            int clearedCount = 0;

            foreach (ObjectId id in modelSpace)
            {
                var entity = trans.GetObject(id, OpenMode.ForWrite) as Entity;
                if (entity?.XData != null)
                {
                    entity.XData = null;
                    clearedCount++;
                }
            }

            Logger.Info($"已清除 {clearedCount} 个零件的关联关系");
        }

        #region 辅助方法

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

        private List<DependencyInfo> ParseDependencyXData(ResultBuffer xdata)
        {
            var deps = new List<DependencyInfo>();
            
            // 解析扩展数据
            // 具体实现根据 XData 格式

            return deps;
        }

        private ResultBuffer MergeXData(ResultBuffer existing, ResultBuffer newData)
        {
            // 合并两个扩展数据缓冲区
            var merged = new ResultBuffer();
            
            foreach (var tv in existing)
            {
                merged.Add(tv);
            }
            
            foreach (var tv in newData)
            {
                merged.Add(tv);
            }

            return merged;
        }

        private ChangeParameters ParseChangeParameters(ResultBuffer xdata)
        {
            // 解析变更参数
            return new ChangeParameters();
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
