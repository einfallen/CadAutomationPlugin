#if !CLOUD_BUILD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using CadAutomationPlugin.Core.Unfold;
using Shared.Logging;

namespace CadAutomationPlugin.Commands
{
    /// <summary>
    /// 展开图生成命令（功能 4）
    /// 用法：在 AutoCAD 命令行输入 GENUNFOLD
    /// </summary>
    public class UnfoldCommands
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(UnfoldCommands));
        private readonly UnfoldEngine _unfoldEngine;

        public UnfoldCommands()
        {
            _unfoldEngine = new UnfoldEngine();
        }

        /// <summary>
        /// 一键生成展开图
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("GENUNFOLD", "生成展开图")]
        public void GenerateUnfold()
        {
            try
            {
                Logger.Info("启动展开图生成命令");
                
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    Application.ShowAlertDialog("没有活动的文档");
                    return;
                }

                var editor = doc.Editor;
                editor.WriteMessage("\n选择要展开的钣金件...");

                var options = new PromptSelectionOptions
                {
                    MessageForAdding = "\n选择钣金零件："
                };

                var result = editor.GetSelection(options);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                // 生成展开图
                var unfoldResult = _unfoldEngine.GenerateUnfoldDrawing(
                    doc.Database,
                    result.Value);

                editor.WriteMessage($"\n✓ 展开图已生成");
                editor.WriteMessage($"\n  展开长度：{unfoldResult.FlatLength:F2} mm");
                editor.WriteMessage($"\n  折弯次数：{unfoldResult.BendCount}");
                editor.WriteMessage($"\n  材料利用率：{unfoldResult.MaterialUtilization:P1}");

                Logger.Info($"展开图生成完成：{unfoldResult.FlatLength:F2}mm");
            }
            catch (Exception ex)
            {
                Logger.Error("展开图生成失败", ex);
                Application.ShowAlertDialog($"生成失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 批量展开
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("BATCHUNFOLD", "批量展开")]
        public void BatchUnfold()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var editor = doc.Editor;
                editor.WriteMessage("\n选择多个钣金件...");

                var options = new PromptSelectionOptions
                {
                    MessageForAdding = "\n选择钣金零件（可多选）："
                };

                var result = editor.GetSelection(options);
                if (result.Status != PromptStatus.OK) return;

                var results = _unfoldEngine.BatchUnfold(doc.Database, result.Value);

                editor.WriteMessage($"\n✓ 批量展开完成：{results.Count} 个零件");
                foreach (var r in results)
                {
                    editor.WriteMessage($"\n  {r.PartName}: {r.FlatLength:F2}mm, {r.BendCount} 折弯");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("批量展开失败", ex);
                Application.ShowAlertDialog($"操作失败：{ex.Message}");
            }
        }
    }
}

#endif
