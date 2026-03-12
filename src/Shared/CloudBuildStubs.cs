// 云编译存根 - 提供 AutoCAD 命名空间的空定义
// 仅在 CLOUD_BUILD 模式下编译

namespace Autodesk.AutoCAD.DatabaseServices
{
    // 空存根类 - 仅用于云编译通过
    public class Database { }
    public class Transaction { }
    public class Entity { }
    public class BlockTable { }
    public class BlockTableRecord { }
    public class SelectionSet { }
    public class SelectedObject { }
    public enum SelectionMode { Normal, Window, Crossing }
    public enum OpenMode { ForRead, ForWrite }
    public class ObjectId { public bool IsNull => true; }
    public class ResultBuffer { }
    public class TypedValue { }
    public enum DxfCode { ExtendedDataRegAppName, ExtendedDataAsciiString, ExtendedDataReal }
    public class RotatedDimension : Entity { }
    public class DiametricDimension : Entity { }
    public class RadialDimension : Entity { }
    public class Dimension : Entity { }
    public class Line : Entity { }
    public class Circle : Entity { }
    public class Arc : Entity { }
    public class Polyline : Entity { }
    public class Solid3d : Entity { }
    public class DBText : Entity { }
    public class DBObject { }
    public class BlockReference : Entity { }
    public enum SegmentType { Line, Arc }
}

namespace Autodesk.AutoCAD.Runtime
{
    public interface IExtensionApplication
    {
        void Initialize();
        void Terminate();
    }
    
    public class Exception : System.Exception
    {
        public object ErrorStatus { get; } = "Unknown";
    }
    
    public sealed class CommandMethodAttribute : System.Attribute
    {
        public CommandMethodAttribute(string name) { }
    }
}

namespace Autodesk.AutoCAD.ApplicationServices
{
    public class Application
    {
        public static DocumentManager DocumentManager { get; } = new DocumentManager();
    }
    
    public class DocumentManager
    {
        public Document MdiActiveDocument { get; } = new Document();
    }
    
    public class Document
    {
        public Editor Editor { get; } = new Editor();
    }
    
    public class Editor
    {
        public void WriteMessage(string message) { }
    }
}

namespace Autodesk.AutoCAD.EditorInput
{
    public class Editor { }
}

namespace Autodesk.AutoCAD.Geometry
{
    public struct Point3d
    {
        public static Point3d Origin => new Point3d();
        public Point3d(double x, double y, double z) { }
        public Point3d GetVectorTo(Point3d other) => new Point3d();
    }
    
    public struct Vector3d
    {
        public Vector3d(double x, double y, double z) { }
        public static Vector3d Zero => new Vector3d();
    }
    
    public struct Point2d
    {
        public static Point2d Origin => new Point2d();
        public Point2d(double x, double y) { }
    }
    
    public struct Vector2d
    {
        public Vector2d(double x, double y) { }
        public static Vector2d Zero => new Vector2d();
    }
}

namespace Autodesk.AutoCAD.GraphicsInterface
{
    public class Viewport { }
}

namespace Autodesk.AutoCAD.PlottingServices
{
    public class PlotEngine { }
}
