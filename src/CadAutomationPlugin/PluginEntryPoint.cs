using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using CadAutomationPlugin.UI.ViewModels;
using CadAutomationPlugin.Core.SmartDimension;
using CadAutomationPlugin.Core.BOM;
using CadAutomationPlugin.Core.ChangePropagation;
using Shared.Logging;
using System.Windows;

[assembly: ExtensionApplication(typeof(CadAutomationPlugin.PluginEntryPoint))]

namespace CadAutomationPlugin
{
    /// <summary>
    /// 插件入口点 - AutoCAD 加载时自动初始化
    /// </summary>
    public class PluginEntryPoint : IExtensionApplication
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(PluginEntryPoint));
        private MainWindow? _mainWindow;

        public void Initialize()
        {
            try
            {
                Logger.Info("CAD 自动化插件正在初始化...");
                
                // 注册命令
                RegisterCommands();
                
                // 初始化核心服务
                InitializeServices();
                
                Logger.Info("CAD 自动化插件初始化完成");
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage("\n✓ CAD 自动化插件已加载 (输入 CADAUTO 启动)");
            }
            catch (Exception ex)
            {
                Logger.Error("插件初始化失败", ex);
                Application.DocumentManager.MdiActiveDocument?.Editor.WriteMessage($"\n✗ 插件加载失败：{ex.Message}");
            }
        }

        public void Terminate()
        {
            Logger.Info("插件正在终止...");
            _mainWindow?.Close();
            Logger.Info("插件已终止");
        }

        private void RegisterCommands()
        {
            // 命令注册在 Commands 文件夹中通过 CommandMethod 特性完成
        }

        private void InitializeServices()
        {
            // 初始化日志
            LogManager.Initialize();
            
            // 初始化数据库连接
            // Data.Database.DbInitializer.Initialize();
            
            // 加载配置
            // ConfigManager.Load();
        }

        /// <summary>
        /// 显示主窗口
        /// </summary>
        public void ShowMainWindow()
        {
            if (_mainWindow == null || !_mainWindow.IsVisible)
            {
                _mainWindow = new MainWindow
                {
                    Owner = Application.Current.MainWindow,
                    DataContext = new MainViewModel()
                };
                _mainWindow.Show();
            }
            else
            {
                _mainWindow.Activate();
            }
        }
    }
}
