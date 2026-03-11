using Autodesk.AutoCAD.ApplicationServices;
using CadAutomationPlugin.Core.Parametric;
using Shared.Logging;

namespace CadAutomationPlugin.Commands
{
    /// <summary>
    /// 参数化图纸生成命令（功能 5/7）
    /// 用法：在 AutoCAD 命令行输入 GENDRAWING
    /// </summary>
    public class ParametricCommands
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(ParametricCommands));
        private readonly ParametricDrawingGenerator _generator;

        public ParametricCommands()
        {
            _generator = new ParametricDrawingGenerator();
        }

        /// <summary>
        /// 根据参数生成图纸
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("GENDRAWING", "参数化出图")]
        public void GenerateDrawing()
        {
            try
            {
                Logger.Info("启动参数化出图命令");
                
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    Application.ShowAlertDialog("没有活动的文档");
                    return;
                }

                // 打开参数输入窗口
                var paramWindow = new UI.Views.ParametricInputWindow
                {
                    Owner = Application.Current.MainWindow
                };

                if (paramWindow.ShowDialog() == true)
                {
                    var parameters = paramWindow.Parameters;
                    
                    // 根据模板生成图纸
                    var result = _generator.GenerateFromTemplate(
                        doc.Database,
                        parameters.TemplatePath,
                        parameters);

                    doc.Editor.WriteMessage($"\n✓ 图纸已生成：{result.DrawingPath}");
                    Logger.Info($"参数化图纸生成完成：{result.DrawingPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("参数化出图失败", ex);
                Application.ShowAlertDialog($"生成失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 生成工艺文件
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("GENPROCESS", "生成工艺文件")]
        public void GenerateProcessDocument()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                // 打开工艺参数输入窗口
                var processWindow = new UI.Views.ProcessParameterWindow
                {
                    Owner = Application.Current.MainWindow
                };

                if (processWindow.ShowDialog() == true)
                {
                    var parameters = processWindow.Parameters;
                    
                    var result = _generator.GenerateProcessDocument(
                        parameters.TemplatePath,
                        parameters);

                    doc.Editor.WriteMessage($"\n✓ 工艺文件已生成：{result.DocumentPath}");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("工艺文件生成失败", ex);
                Application.ShowAlertDialog($"生成失败：{ex.Message}");
            }
        }
    }
}
