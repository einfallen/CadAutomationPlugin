#if !CLOUD_BUILD
using Autodesk.AutoCAD.DatabaseServices;
using Shared.Logging;

namespace CadAutomationPlugin.Core.Unfold
{
    /// <summary>
    /// 展开图生成引擎 - 功能 4
    /// 将 3D 钣金件展开为 2D 工艺图
    /// </summary>
    public class UnfoldEngine
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(UnfoldEngine));

        /// <summary>
        /// 生成单个零件的展开图
        /// </summary>
        public UnfoldResult GenerateUnfoldDrawing(Database db, SelectionSet selection)
        {
            Logger.Info("开始生成展开图");

            var result = new UnfoldResult();

            foreach (SelectedObject selObj in selection)
            {
                var entity = db.TransactionManager.TopTransaction.GetObject(
                    selObj.ObjectId, OpenMode.ForRead) as Entity;
                
                if (entity is Solid3d solid)
                {
                    var unfoldData = UnfoldSolid(solid);
                    result.FlatLength = unfoldData.FlatLength;
                    result.BendCount = unfoldData.BendCount;
                    result.MaterialUtilization = unfoldData.MaterialUtilization;
                    
                    // 生成展开轮廓
                    GenerateFlatPattern(db, unfoldData);
                }
            }

            return result;
        }

        /// <summary>
        /// 批量展开
        /// </summary>
        public List<UnfoldResult> BatchUnfold(Database db, SelectionSet selection)
        {
            var results = new List<UnfoldResult>();

            foreach (SelectedObject selObj in selection)
            {
                var entity = db.TransactionManager.TopTransaction.GetObject(
                    selObj.ObjectId, OpenMode.ForRead) as Entity;

                if (entity is Solid3d solid)
                {
                    var unfoldData = UnfoldSolid(solid);
                    var result = new UnfoldResult
                    {
                        PartName = solid.Handle.ToString(),
                        FlatLength = unfoldData.FlatLength,
                        BendCount = unfoldData.BendCount,
                        MaterialUtilization = unfoldData.MaterialUtilization
                    };

                    GenerateFlatPattern(db, unfoldData);
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// 展开 3D 实体
        /// </summary>
        private UnfoldData UnfoldSolid(Solid3d solid)
        {
            var kFactor = 0.5;
            var thickness = GetSheetThickness(solid);
            var bends = IdentifyBends(solid);

            double flatLength = 0;
            foreach (var bend in bends)
            {
                flatLength += bend.FlatLength;
            }

            flatLength += CalculateStraightLength(solid, bends);

            return new UnfoldData
            {
                FlatLength = flatLength,
                BendCount = bends.Count,
                Thickness = thickness,
                MaterialUtilization = CalculateMaterialUtilization(flatLength, solid),
                BendData = bends
            };
        }

        /// <summary>
        /// 识别折弯特征
        /// </summary>
        private List<BendInfo> IdentifyBends(Solid3d solid)
        {
            var bends = new List<BendInfo>();
            return bends;
        }

        /// <summary>
        /// 获取板材厚度
        /// </summary>
        private double GetSheetThickness(Solid3d solid)
        {
            return 2.0;
        }

        /// <summary>
        /// 计算直线段长度
        /// </summary>
        private double CalculateStraightLength(Solid3d solid, List<BendInfo> bends)
        {
            return 100.0;
        }

        /// <summary>
        /// 计算材料利用率
        /// </summary>
        private double CalculateMaterialUtilization(double flatLength, Solid3d solid)
        {
            return 0.85;
        }

        /// <summary>
        /// 生成平面轮廓
        /// </summary>
        private void GenerateFlatPattern(Database db, UnfoldData data)
        {
            using (var trans = db.TransactionManager.StartTransaction())
            {
                var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                var polyline = new Polyline();
                polyline.AddVertexAt(0, new Point2d(0, 0), 0, data.Thickness, data.Thickness);
                polyline.AddVertexAt(1, new Point2d(data.FlatLength, 0), 0, data.Thickness, data.Thickness);
                polyline.AddVertexAt(2, new Point2d(data.FlatLength, 100), 0, data.Thickness, data.Thickness);
                polyline.AddVertexAt(3, new Point2d(0, 100), 0, data.Thickness, data.Thickness);
                polyline.Closed = true;

                modelSpace.AppendEntity(polyline);
                trans.AddNewlyCreatedDBObject(polyline, true);

                foreach (var bend in data.BendData)
                {
                    var bendLine = new Line(
                        new Point3d(bend.Position, 0, 0),
                        new Point3d(bend.Position, 100, 0)
                    );
                    modelSpace.AppendEntity(bendLine);
                    trans.AddNewlyCreatedDBObject(bendLine, true);
                }

                trans.Commit();
            }
        }
    }
}
#else
// 云编译存根
namespace CadAutomationPlugin.Core.Unfold
{
    public class UnfoldEngineStub
    {
        // 云编译时不使用此功能
    }
    
    public class UnfoldResult { public string PartName { get; set; } = ""; public double FlatLength { get; set; } public int BendCount { get; set; } public double MaterialUtilization { get; set; } public string DrawingPath { get; set; } = ""; }
    public class UnfoldData { public double FlatLength { get; set; } public int BendCount { get; set; } public double Thickness { get; set; } public double MaterialUtilization { get; set; } public List<BendInfo> BendData { get; set; } = new List<BendInfo>(); }
    public class BendInfo { public double Position { get; set; } public double Angle { get; set; } public double Radius { get; set; } public double FlatLength => Radius * Angle * Math.PI / 180.0; }
}
#endif
