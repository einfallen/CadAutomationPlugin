// Core 项目云编译存根 - 提供公共类的空定义
// 文件通过 Core.csproj 条件包含/移除控制

namespace CadAutomationPlugin.Core.BOM
{
    public class BOMGeneratorStub { }
    
    public class BOMData
    {
        public System.Collections.Generic.List<BOMItem> Items { get; set; } = new System.Collections.Generic.List<BOMItem>();
        public string ProjectName { get; set; } = "";
        public string DrawingNumber { get; set; } = "";
        public System.DateTime GeneratedAt { get; set; } = System.DateTime.Now;
        public string GeneratedBy { get; set; } = "";
    }

    public class BOMItem
    {
        public int Index { get; set; }
        public string PartNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public string Material { get; set; } = "";
        public double UnitWeight { get; set; }
        public double TotalWeight { get; set; }
        public string Vendor { get; set; } = "";
        public string Description { get; set; } = "";
        public string Remarks { get; set; } = "";
    }
}

namespace CadAutomationPlugin.Core.ChangePropagation
{
    public class ChangePropagationEngineStub { }
    public class AffectedPart { }
    public class DependencyInfo { }
    public class ChangeParameters { }
}

namespace CadAutomationPlugin.Core.Parametric
{
    public class ParametricDrawingGeneratorStub { }
    public class ParametricParameters { }
    public class ProcessParameters { }
    public class Parameter { }
    public class DrawingResult { }
    public class DocumentResult { }
}

namespace CadAutomationPlugin.Core.SmartDimension
{
    public class SmartDimensionEngineStub { }
    public class DimensionConfig { }
}

namespace CadAutomationPlugin.Core.Unfold
{
    public class UnfoldEngineStub { }
    public class UnfoldResult 
    { 
        public string PartName { get; set; } = ""; 
        public double FlatLength { get; set; } 
        public int BendCount { get; set; } 
        public double MaterialUtilization { get; set; } 
        public string DrawingPath { get; set; } = ""; 
    }
    public class UnfoldData 
    { 
        public double FlatLength { get; set; } 
        public int BendCount { get; set; } 
        public double Thickness { get; set; } 
        public double MaterialUtilization { get; set; } 
        public System.Collections.Generic.List<BendInfo> BendData { get; set; } = new System.Collections.Generic.List<BendInfo>(); 
    }
    public class BendInfo 
    { 
        public double Position { get; set; } 
        public double Angle { get; set; } 
        public double Radius { get; set; } 
        public double FlatLength => Radius * Angle * Math.PI / 180.0; 
    }
}
