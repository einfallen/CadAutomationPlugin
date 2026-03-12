// UI 项目云编译存根 - 提供 ViewModels 的空定义
// 仅在 CLOUD_BUILD 模式下编译

#if CLOUD_BUILD

namespace CadAutomationPlugin.UI.ViewModels
{
    public class MainViewModelStub { }
}

namespace CadAutomationPlugin.UI.Views
{
    public class MainWindowStub { }
}

#endif
