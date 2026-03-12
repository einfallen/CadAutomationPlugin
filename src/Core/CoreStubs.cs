// Core 项目云编译存根 - 提供公共类的空定义
// 仅在 CLOUD_BUILD 模式下编译
// 注意：BOM 和 Unfold 的存根类已在各自文件中定义，这里只包含其他模块的存根

#if CLOUD_BUILD

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

#endif
