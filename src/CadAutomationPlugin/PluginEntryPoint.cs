#if !CLOUD_BUILD
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

        /// <summary>
        /// 插件初始化 - AutoCAD 加载时调用
        /// </summary>
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
                WriteToEditor("✓ CAD 自动化插件已加载 (输入 CADAUTO 启动)");
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"插件初始化失败 (AutoCAD 错误：{ex.ErrorStatus})", ex);
                WriteToEditor($"✗ 插件加载失败：{ex.Message}");
            }
            catch (System.Exception ex)
            {
                Logger.Error("插件初始化失败 (未知错误)", ex);
                WriteToEditor($"✗ 插件加载失败：{ex.Message}");
                throw; // 重新抛出未知异常
            }
        }

        /// <summary>
        /// 插件终止 - AutoCAD 卸载时调用
        /// </summary>
        public void Terminate()
        {
            try
            {
                Logger.Info("插件正在终止...");
                _mainWindow?.Close();
                _mainWindow = null;
                Logger.Info("插件已终止");
            }
            catch (System.Exception ex)
            {
                Logger.Error("插件终止时发生错误", ex);
            }
        }

        /// <summary>
        /// 注册命令
        /// </summary>
        private void RegisterCommands()
        {
            // 命令注册在 Commands 文件夹中通过 CommandMethod 特性完成
            Logger.Debug("命令已注册");
        }

        /// <summary>
        /// 初始化核心服务
        /// </summary>
        private void InitializeServices()
        {
            try
            {
                // 初始化日志
                LogManager.Initialize();
                Logger.Debug("日志系统已初始化");
                
                // 初始化数据库连接
                // Data.Database.DbInitializer.Initialize();
                
                // 加载配置
                // ConfigManager.Load();
                
                Logger.Debug("核心服务已初始化");
            }
            catch (System.Exception ex)
            {
                Logger.Error("初始化核心服务失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 显示主窗口
        /// </summary>
        public void ShowMainWindow()
        {
            try
            {
                if (_mainWindow == null || !_mainWindow.IsVisible)
                {
                    _mainWindow = new MainWindow
                    {
                        Owner = Application.Current.MainWindow,
                        DataContext = new MainViewModel()
                    };
                    _mainWindow.Show();
                    Logger.Info("主窗口已显示");
                }
                else
                {
                    _mainWindow.Activate();
                    Logger.Info("主窗口已激活");
                }
            }
            catch (System.Exception ex)
            {
                Logger.Error("显示主窗口失败", ex);
                WriteToEditor($"✗ 无法显示主窗口：{ex.Message}");
            }
        }

        /// <summary>
        /// 向 AutoCAD 编辑器写入消息（安全包装）
        /// </summary>
        private void WriteToEditor(string message)
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                doc?.Editor.WriteMessage($"\n{message}");
            }
            catch (System.Exception ex)
            {
                Logger.Error("写入编辑器失败", ex);
                // 静默失败，不影响主流程
            }
        }
    }
}
#else
// 云编译存根 - 跳过 AutoCAD 插件入口点
namespace CadAutomationPlugin
{
    public class PluginEntryPointStub { }
}
#endif
