using Autodesk.AutoCAD.DatabaseServices;
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
        
        /// <summary>
        /// 标注配置
        /// </summary>
        public DimensionConfig Config { get; set; } = new DimensionConfig();

        /// <summary>
        /// 自动标注主入口
        /// </summary>
        public void AutoDimension(Database db, SelectionSet selection, Transaction trans)
        {
            Logger.Info($"开始智能标注，选中 {selection.Count} 个对象");

            foreach (SelectedObject selObj in selection)
            {
                var entity = trans.GetObject(selObj.ObjectId, OpenMode.ForRead) as Entity;
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
        }

        /// <summary>
        /// 标注直线
        /// </summary>
        private void DimensionLine(Line line, Database db, Transaction trans)
        {
            try
            {
                var start = line.StartPoint;
                var end = line.EndPoint;
                var length = line.Length;

                // 创建线性标注
                var dim = new RotatedDimension
                {
                    DefPoint = start,
                    XLine1Point = start,
                    XLine2Point = end,
                    Rotation = line.Angle,
                    DimensionStyle = GetDefaultStyle(db)
                };

                // 智能放置标注位置（避免重叠）
                dim.TextPosition = CalculateOptimalTextPosition(line);

                var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                modelSpace.AppendEntity(dim);
                trans.AddNewlyCreatedDBObject(dim, true);

                Logger.Debug($"标注直线：{length:F2}");
            }
            catch (Exception ex)
            {
                Logger.Error("直线标注失败", ex);
            }
        }

        /// <summary>
        /// 标注圆
        /// </summary>
        private void DimensionCircle(Circle circle, Database db, Transaction trans)
        {
            try
            {
                var diameter = circle.Radius * 2;

                // 创建直径标注
                var dim = new DiametricDimension
                {
                    DefPoint = circle.Center,
                    XLine1Point = circle.Center + new Point3d(circle.Radius, 0, 0),
                    XLine2Point = circle.Center + new Point3d(-circle.Radius, 0, 0),
                    DimensionStyle = GetDefaultStyle(db)
                };

                var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                modelSpace.AppendEntity(dim);
                trans.AddNewlyCreatedDBObject(dim, true);

                Logger.Debug($"标注圆：Φ{diameter:F2}");
            }
            catch (Exception ex)
            {
                Logger.Error("圆标注失败", ex);
            }
        }

        /// <summary>
        /// 标注圆弧
        /// </summary>
        private void DimensionArc(Arc arc, Database db, Transaction trans)
        {
            try
            {
                var radius = arc.Radius;

                // 创建半径标注
                var dim = new RadialDimension
                {
                    DefPoint = arc.Center,
                    XLine1Point = arc.Center + new Point3d(arc.Radius * Math.Cos(arc.StartAngle), arc.Radius * Math.Sin(arc.StartAngle), 0),
                    DimensionStyle = GetDefaultStyle(db)
                };

                var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                modelSpace.AppendEntity(dim);
                trans.AddNewlyCreatedDBObject(dim, true);

                Logger.Debug($"标注圆弧：R{radius:F2}");
            }
            catch (Exception ex)
            {
                Logger.Error("圆弧标注失败", ex);
            }
        }

        /// <summary>
        /// 标注多段线
        /// </summary>
        private void DimensionPolyline(Polyline pline, Database db, Transaction trans)
        {
            try
            {
                // 遍历多段线顶点
                for (int i = 0; i < pline.NumberOfVertices - 1; i++)
                {
                    var segType = pline.GetSegmentType(i);
                    var seg = pline.GetLineSegmentAt(i);

                    if (segType == SegmentType.Line)
                    {
                        // 标注直线段
                        var dim = new RotatedDimension
                        {
                            DefPoint = seg.StartPoint,
                            XLine1Point = seg.StartPoint,
                            XLine2Point = seg.EndPoint,
                            Rotation = seg.Angle,
                            DimensionStyle = GetDefaultStyle(db)
                        };

                        var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                        var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
                        modelSpace.AppendEntity(dim);
                        trans.AddNewlyCreatedDBObject(dim, true);
                    }
                }

                Logger.Debug($"标注多段线：{pline.NumberOfVertices} 个顶点");
            }
            catch (Exception ex)
            {
                Logger.Error("多段线标注失败", ex);
            }
        }

        /// <summary>
        /// 标注孔特征
        /// </summary>
        public void DimensionHoles(Database db, Transaction trans)
        {
            Logger.Info("开始标注孔特征");

            var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;

            foreach (ObjectId id in modelSpace)
            {
                var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                if (entity is Circle circle)
                {
                    // 判断是否为孔（根据半径范围）
                    if (circle.Radius >= Config.MinHoleRadius && circle.Radius <= Config.MaxHoleRadius)
                    {
                        DimensionCircle(circle, db, trans);
                    }
                }
            }
        }

        /// <summary>
        /// 标注倒角和圆角
        /// </summary>
        public void DimensionChamfersAndFillets(Database db, Transaction trans)
        {
            Logger.Info("开始标注倒角/圆角");

            // 实现倒角和圆角的识别与标注逻辑
            // 需要分析相邻线段的角度和距离
        }

        /// <summary>
        /// 清除所有标注
        /// </summary>
        public void ClearAllDimensions(Database db, Transaction trans)
        {
            var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

            var dimsToRemove = new List<ObjectId>();

            foreach (ObjectId id in modelSpace)
            {
                var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                if (entity is Dimension)
                {
                    dimsToRemove.Add(id);
                }
            }

            foreach (var dimId in dimsToRemove)
            {
                dimId.Erase();
            }

            Logger.Info($"已清除 {dimsToRemove.Count} 个标注");
        }

        /// <summary>
        /// 计算最优标注文本位置
        /// </summary>
        private Point3d CalculateOptimalTextPosition(Line line)
        {
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
    }

    /// <summary>
    /// 标注配置
    /// </summary>
    public class DimensionConfig
    {
        public double TextOffset { get; set; } = 5.0;
        public double MinHoleRadius { get; set; } = 1.0;
        public double MaxHoleRadius { get; set; } = 100.0;
        public string DimensionStyleName { get; set; } = "Standard";
        public double Precision { get; set; } = 0.01;
    }
}
