#if !CLOUD_BUILD
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Shared.Logging;
using Shared.Geometry;

namespace CadAutomationPlugin.Core.SmartDimension
{
    /// <summary>
    /// 智能标注引擎 - 核心功能 1
    /// 自动识别几何特征并生成符合国标的标注
    /// </summary>
    public class SmartDimensionEngine
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(SmartDimensionEngine));
        
        // 配置常量 - 替代魔法数字
        private const double DefaultTextOffset = 5.0;
        private const double DefaultMinHoleRadius = 1.0;
        private const double DefaultMaxHoleRadius = 100.0;
        private const double DefaultPrecision = 0.01;

        /// <summary>
        /// 标注配置
        /// </summary>
        public DimensionConfig Config { get; set; } = new DimensionConfig();

        /// <summary>
        /// 自动标注主入口
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="selection">选择的对象集</param>
        /// <param name="trans">当前事务</param>
        /// <exception cref="ArgumentNullException">当 db、selection 或 trans 为 null 时</exception>
        public void AutoDimension(Database db, SelectionSet selection, Transaction trans)
        {
            // 参数验证
            if (db == null)
            {
                Logger.Error("AutoDimension: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (selection == null)
            {
                Logger.Warn("AutoDimension: selection 为 null，无操作");
                return;
            }

            if (selection.Count == 0)
            {
                Logger.Warn("AutoDimension: 未选择任何对象");
                return;
            }

            if (trans == null)
            {
                Logger.Error("AutoDimension: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            Logger.Info($"开始智能标注，选中 {selection.Count} 个对象");

            foreach (SelectedObject selObj in selection)
            {
                try
                {
                    using var entity = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
                    if (entity == null) continue;

                    // 根据实体类型进行标注
                    switch (entity)
                    {
                        case Line line:
                            DimensionLine(line, db, trans);
                            break;
                        case Circle circle:
                            DimensionCircle(circle, db, trans);
                            break;
                        case Arc arc:
                            DimensionArc(arc, db, trans);
                            break;
                        case Polyline pline:
                            DimensionPolyline(pline, db, trans);
                            break;
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"处理对象时发生 AutoCAD 错误：{ex.ErrorStatus}", ex);
                }
                catch (System.Exception ex)
                {
                    Logger.Error($"处理对象时发生未知错误", ex);
                    throw; // 重新抛出未知异常
                }
            }
        }

        /// <summary>
        /// 标注直线
        /// </summary>
        private void DimensionLine(Line line, Database db, Transaction trans)
        {
            if (line == null || line.IsDisposed)
            {
                Logger.Warn("DimensionLine: 直线对象无效");
                return;
            }

            try
            {
                var start = line.StartPoint;
                var end = line.EndPoint;
                var length = line.Length;

                // 创建线性标注
                using var dim = new RotatedDimension
                {
                    DefPoint = start,
                    XLine1Point = start,
                    XLine2Point = end,
                    Rotation = line.Angle,
                    DimensionStyle = GetDefaultStyle(db)
                };

                // 智能放置标注位置（避免重叠）
                dim.TextPosition = CalculateOptimalTextPosition(line);

                AddToModelSpace(dim, db, trans);

                Logger.Debug($"标注直线：{length:F2}");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"直线标注失败：{ex.ErrorStatus}", ex);
            }
            catch (System.Exception ex)
            {
                Logger.Error("直线标注失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 标注圆
        /// </summary>
        private void DimensionCircle(Circle circle, Database db, Transaction trans)
        {
            if (circle == null || circle.IsDisposed)
            {
                Logger.Warn("DimensionCircle: 圆对象无效");
                return;
            }

            try
            {
                var diameter = circle.Radius * 2;

                // 创建直径标注
                using var dim = new DiametricDimension
                {
                    DefPoint = circle.Center,
                    XLine1Point = circle.Center + new Point3d(circle.Radius, 0, 0),
                    XLine2Point = circle.Center + new Point3d(-circle.Radius, 0, 0),
                    DimensionStyle = GetDefaultStyle(db)
                };

                AddToModelSpace(dim, db, trans);

                Logger.Debug($"标注圆：Φ{diameter:F2}");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"圆标注失败：{ex.ErrorStatus}", ex);
            }
            catch (System.Exception ex)
            {
                Logger.Error("圆标注失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 标注圆弧
        /// </summary>
        private void DimensionArc(Arc arc, Database db, Transaction trans)
        {
            if (arc == null || arc.IsDisposed)
            {
                Logger.Warn("DimensionArc: 圆弧对象无效");
                return;
            }

            try
            {
                var radius = arc.Radius;

                // 创建半径标注
                using var dim = new RadialDimension
                {
                    DefPoint = arc.Center,
                    XLine1Point = arc.Center + new Point3d(arc.Radius * Math.Cos(arc.StartAngle), arc.Radius * Math.Sin(arc.StartAngle), 0),
                    DimensionStyle = GetDefaultStyle(db)
                };

                AddToModelSpace(dim, db, trans);

                Logger.Debug($"标注圆弧：R{radius:F2}");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"圆弧标注失败：{ex.ErrorStatus}", ex);
            }
            catch (System.Exception ex)
            {
                Logger.Error("圆弧标注失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 标注多段线
        /// </summary>
        private void DimensionPolyline(Polyline pline, Database db, Transaction trans)
        {
            if (pline == null || pline.IsDisposed)
            {
                Logger.Warn("DimensionPolyline: 多段线对象无效");
                return;
            }

            try
            {
                // 遍历多段线顶点
                for (int i = 0; i < pline.NumberOfVertices - 1; i++)
                {
                    var segType = pline.GetSegmentType(i);
                    
                    if (segType == SegmentType.Line)
                    {
                        using var seg = pline.GetLineSegmentAt(i);
                        
                        // 标注直线段
                        using var dim = new RotatedDimension
                        {
                            DefPoint = seg.StartPoint,
                            XLine1Point = seg.StartPoint,
                            XLine2Point = seg.EndPoint,
                            Rotation = seg.Angle,
                            DimensionStyle = GetDefaultStyle(db)
                        };

                        AddToModelSpace(dim, db, trans);
                    }
                }

                Logger.Debug($"标注多段线：{pline.NumberOfVertices} 个顶点");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"多段线标注失败：{ex.ErrorStatus}", ex);
            }
            catch (System.Exception ex)
            {
                Logger.Error("多段线标注失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 标注孔特征
        /// </summary>
        public void DimensionHoles(Database db, Transaction trans)
        {
            if (db == null)
            {
                Logger.Error("DimensionHoles: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (trans == null)
            {
                Logger.Error("DimensionHoles: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            Logger.Info("开始标注孔特征");

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null)
            {
                Logger.Error("DimensionHoles: 无法获取块表");
                return;
            }

            using var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            if (modelSpace == null)
            {
                Logger.Error("DimensionHoles: 无法获取模型空间");
                return;
            }

            foreach (ObjectId id in modelSpace)
            {
                try
                {
                    using var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity is Circle circle)
                    {
                        // 判断是否为孔（根据半径范围）
                        if (circle.Radius >= Config.MinHoleRadius && circle.Radius <= Config.MaxHoleRadius)
                        {
                            DimensionCircle(circle, db, trans);
                        }
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"处理孔特征时发生错误：{ex.ErrorStatus}", ex);
                }
            }
        }

        /// <summary>
        /// 标注倒角和圆角
        /// </summary>
        /// <remarks>
        /// TODO: 实现倒角和圆角的识别与标注逻辑
        /// 需要分析相邻线段的角度和距离
        /// 计划实现时间：2026-Q2
        /// </remarks>
        public void DimensionChamfersAndFillets(Database db, Transaction trans)
        {
            if (db == null)
            {
                Logger.Error("DimensionChamfersAndFillets: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (trans == null)
            {
                Logger.Error("DimensionChamfersAndFillets: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            Logger.Info("开始标注倒角/圆角");
            
            // TODO: 实现倒角和圆角的识别与标注逻辑
            // 需要分析相邻线段的角度和距离
            Logger.Warn("DimensionChamfersAndFillets: 功能尚未实现");
        }

        /// <summary>
        /// 清除所有标注
        /// </summary>
        public void ClearAllDimensions(Database db, Transaction trans)
        {
            if (db == null)
            {
                Logger.Error("ClearAllDimensions: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (trans == null)
            {
                Logger.Error("ClearAllDimensions: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null)
            {
                Logger.Error("ClearAllDimensions: 无法获取块表");
                return;
            }

            using var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            if (modelSpace == null)
            {
                Logger.Error("ClearAllDimensions: 无法获取模型空间");
                return;
            }

            var dimsToRemove = new List<ObjectId>();

            foreach (ObjectId id in modelSpace)
            {
                try
                {
                    using var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity is Dimension)
                    {
                        dimsToRemove.Add(id);
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"读取对象失败：{ex.ErrorStatus}", ex);
                }
            }

            foreach (var dimId in dimsToRemove)
            {
                try
                {
                    dimId.Erase();
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"删除标注失败：{ex.ErrorStatus}", ex);
                }
            }

            Logger.Info($"已清除 {dimsToRemove.Count} 个标注");
        }

        /// <summary>
        /// 计算最优标注文本位置
        /// </summary>
        private Point3d CalculateOptimalTextPosition(Line line)
        {
            if (line == null)
            {
                return Point3d.Origin;
            }

            var midPoint = line.StartPoint.GetVectorTo(line.EndPoint).GetMidPoint();
            var offset = new Vector3d(0, Config.TextOffset, 0);
            return midPoint + offset;
        }

        /// <summary>
        /// 获取默认标注样式
        /// </summary>
        private ObjectId GetDefaultStyle(Database db)
        {
            // 返回标准标注样式 ID
            return db.DimStyleId;
        }

        /// <summary>
        /// 将实体添加到模型空间（辅助方法，消除代码重复）
        /// </summary>
        /// <param name="entity">要添加的实体</param>
        /// <param name="db">数据库</param>
        /// <param name="trans">事务</param>
        private void AddToModelSpace(Entity entity, Database db, Transaction trans)
        {
            if (entity == null || db == null || trans == null)
            {
                Logger.Warn("AddToModelSpace: 参数无效");
                return;
            }

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null)
            {
                Logger.Error("AddToModelSpace: 无法获取块表");
                return;
            }

            using var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            if (modelSpace == null)
            {
                Logger.Error("AddToModelSpace: 无法获取模型空间");
                return;
            }

            modelSpace.AppendEntity(entity);
            trans.AddNewlyCreatedDBObject(entity, true);
        }
    }

    /// <summary>
    /// 标注配置
    /// </summary>
    public class DimensionConfig
    {
        public double TextOffset { get; set; } = SmartDimensionEngine.DefaultTextOffset;
        public double MinHoleRadius { get; set; } = SmartDimensionEngine.DefaultMinHoleRadius;
        public double MaxHoleRadius { get; set; } = SmartDimensionEngine.DefaultMaxHoleRadius;
        public string DimensionStyleName { get; set; } = "Standard";
        public double Precision { get; set; } = SmartDimensionEngine.DefaultPrecision;
    }
}
#endif
