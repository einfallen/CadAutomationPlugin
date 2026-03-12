// 云编译存根 - 提供 AutoCAD 命名空间的空定义
// 仅在 CLOUD_BUILD 模式下编译

#if CLOUD_BUILD

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
        public static System.Windows.Window Current { get; } = new System.Windows.Window();
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
    }
    
    public struct Vector3d
    {
        public Vector3d(double x, double y, double z) { }
    }
}

#endif
