#if !CLOUD_BUILD
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CadAutomationPlugin.UI.Views
{
    /// <summary>
    /// 主窗口代码后台
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
#endif
