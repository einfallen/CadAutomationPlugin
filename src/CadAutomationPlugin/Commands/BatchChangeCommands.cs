#if !CLOUD_BUILD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using CadAutomationPlugin.Core.ChangePropagation;
using Shared.Logging;

namespace CadAutomationPlugin.Commands
{
    /// <summary>
    /// 批量改图/连锁反应命令
    /// 用法：在 AutoCAD 命令行输入 BATCHCHANGE
    /// </summary>
    public class BatchChangeCommands
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(BatchChangeCommands));
        private readonly ChangePropagationEngine _changeEngine;

        public BatchChangeCommands()
        {
            _changeEngine = new ChangePropagationEngine();
        }

        /// <summary>
        /// 批量修改命令入口
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("BATCHCHANGE", "批量改图")]
        public void BatchChange()
        {
            try
            {
                Logger.Info("启动批量改图命令");
                
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    Application.ShowAlertDialog("没有活动的文档");
                    return;
                }

                var editor = doc.Editor;
                editor.WriteMessage("\n⚠ 批量改图功能将影响多个关联零件，请确认继续...");

                // 提示用户选择基准零件
                var options = new PromptSelectionOptions
                {
                    MessageForAdding = "\n选择要修改的基准零件：",
                    AllowDuplicates = false
                };

                var result = editor.GetSelection(options);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                // 分析关联关系
                editor.WriteMessage("\n正在分析装配关联关系...");
                
                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    var affectedParts = _changeEngine.AnalyzeDependencies(
                        doc.Database, 
                        result.Value, 
                        trans);

                    editor.WriteMessage($"\n⚠ 此修改将影响 {affectedParts.Count} 个关联零件:");
                    foreach (var part in affectedParts)
                    {
                        editor.WriteMessage($"\n  - {part.PartName} ({part.RelationshipType})");
                    }

                    // 确认是否继续
                    var confirmResult = editor.GetString("\n是否继续执行批量修改？[是/否 (Y/N)]: ");
                    if (confirmResult?.ToUpper() != "Y" && confirmResult?.ToUpper() != "是")
                    {
                        editor.WriteMessage("\n操作已取消");
                        return;
                    }

                    // 执行变更传播
                    _changeEngine.PropagateChange(doc.Database, result.Value, trans);
                    trans.Commit();

                    editor.WriteMessage("\n✓ 批量修改完成");
                }

                Logger.Info("批量修改完成");
            }
            catch (Exception ex)
            {
                Logger.Error("批量修改失败", ex);
                Application.ShowAlertDialog($"批量修改失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 查看关联关系
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("SHOWDEPENDENCIES", "查看关联")]
        public void ShowDependencies()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var editor = doc.Editor;
                
                var options = new PromptSelectionOptions
                {
                    MessageForAdding = "\n选择要查看关联的零件："
                };

                var result = editor.GetSelection(options);
                if (result.Status != PromptStatus.OK) return;

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    var dependencies = _changeEngine.GetDependencies(
                        doc.Database, 
                        result.Value, 
                        trans);

                    editor.WriteMessage($"\n📊 关联关系分析:");
                    foreach (var dep in dependencies)
                    {
                        editor.WriteMessage($"\n  {dep.PartName}: {dep.Description}");
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("关联分析失败", ex);
                Application.ShowAlertDialog($"分析失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 注册零件关联
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("REGISTERLINK", "注册关联")]
        public void RegisterLink()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var editor = doc.Editor;

                // 选择父零件
                editor.WriteMessage("\n选择父零件：");
                var parentResult = editor.GetEntity();
                if (parentResult.Status != PromptStatus.OK) return;

                // 选择子零件
                editor.WriteMessage("\n选择子零件：");
                var childResult = editor.GetEntity();
                if (childResult.Status != PromptStatus.OK) return;

                // 选择关联类型
                var linkType = editor.GetString(
                    "\n选择关联类型 [同心/重合/距离/平行/垂直]：");

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    _changeEngine.RegisterDependency(
                        doc.Database,
                        parentResult.ObjectId,
                        childResult.ObjectId,
                        linkType ?? "同心",
                        trans);

                    trans.Commit();
                }

                editor.WriteMessage("\n✓ 关联关系已注册");
            }
            catch (Exception ex)
            {
                Logger.Error("关联注册失败", ex);
                Application.ShowAlertDialog($"注册失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 清除所有关联
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("CLEARLINKS", "清除关联")]
        public void ClearLinks()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var confirmResult = Application.ShowAlertDialog(
                    "确定要清除所有关联关系吗？此操作不可恢复。",
                    "确认",
                    "确认",
                    "取消");

                if (confirmResult == 0)
                {
                    using (var trans = doc.Database.TransactionManager.StartTransaction())
                    {
                        _changeEngine.ClearAllDependencies(doc.Database, trans);
                        trans.Commit();
                    }

                    doc.Editor.WriteMessage("\n✓ 已清除所有关联关系");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("清除关联失败", ex);
                Application.ShowAlertDialog($"操作失败：{ex.Message}");
            }
        }
    }
}

#endif
