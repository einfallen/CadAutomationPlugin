using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using CadAutomationPlugin.Core.SmartDimension;
using Shared.Logging;

namespace CadAutomationPlugin.Commands
{
    /// <summary>
    /// 智能标注命令
    /// 用法：在 AutoCAD 命令行输入 SMARTDIM
    /// </summary>
    public class SmartDimensionCommands
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(SmartDimensionCommands));
        private readonly SmartDimensionEngine _dimensionEngine;

        public SmartDimensionCommands()
        {
            _dimensionEngine = new SmartDimensionEngine();
        }

        /// <summary>
        /// 智能标注命令入口
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("SMARTDIM", "SMARTDIMENSION", "智能标注")]
        public void SmartDimension()
        {
            try
            {
                Logger.Info("启动智能标注命令");
                
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    Application.ShowAlertDialog("没有活动的文档");
                    return;
                }

                var editor = doc.Editor;
                
                // 提示用户选择对象
                var options = new PromptSelectionOptions
                {
                    MessageForAdding = "\n选择要标注的对象：",
                    MessageForRemoving = "\n移除对象："
                };

                var result = editor.GetSelection(options);
                if (result.Status != PromptStatus.OK)
                {
                    return;
                }

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    // 执行智能标注
                    _dimensionEngine.AutoDimension(doc.Database, result.Value, trans);
                    trans.Commit();
                }

                editor.WriteMessage("\n✓ 智能标注完成");
                Logger.Info("智能标注完成");
            }
            catch (Exception ex)
            {
                Logger.Error("智能标注失败", ex);
                Application.ShowAlertDialog($"标注失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 标注孔特征
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("SMARTDIMHOLES", "标注孔")]
        public void DimensionHoles()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    _dimensionEngine.DimensionHoles(doc.Database, trans);
                    trans.Commit();
                }

                doc.Editor.WriteMessage("\n✓ 孔特征标注完成");
            }
            catch (Exception ex)
            {
                Logger.Error("孔标注失败", ex);
                Application.ShowAlertDialog($"标注失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 标注倒角和圆角
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("SMARTDIMCHAMFER", "标注倒角")]
        public void DimensionChamfers()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    _dimensionEngine.DimensionChamfersAndFillets(doc.Database, trans);
                    trans.Commit();
                }

                doc.Editor.WriteMessage("\n✓ 倒角/圆角标注完成");
            }
            catch (Exception ex)
            {
                Logger.Error("倒角标注失败", ex);
                Application.ShowAlertDialog($"标注失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 清除所有标注
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("CLEARALLDIMS", "清除标注")]
        public void ClearAllDimensions()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    _dimensionEngine.ClearAllDimensions(doc.Database, trans);
                    trans.Commit();
                }

                doc.Editor.WriteMessage("\n✓ 已清除所有标注");
            }
            catch (Exception ex)
            {
                Logger.Error("清除标注失败", ex);
                Application.ShowAlertDialog($"操作失败：{ex.Message}");
            }
        }
    }
}
