#if !CLOUD_BUILD
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.DatabaseServices;
using CadAutomationPlugin.Core.BOM;
using CadAutomationPlugin.Data.Excel;
using Shared.Logging;
using System.Windows.Forms;

namespace CadAutomationPlugin.Commands
{
    /// <summary>
    /// BOM 表生成命令
    /// 用法：在 AutoCAD 命令行输入 GENBOM
    /// </summary>
    public class BOMCommands
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(BOMCommands));
        private readonly BOMGenerator _bomGenerator;
        private readonly ExcelExporter _excelExporter;

        public BOMCommands()
        {
            _bomGenerator = new BOMGenerator();
            _excelExporter = new ExcelExporter();
        }

        /// <summary>
        /// 生成 BOM 表命令
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("GENBOM", "GENERATEBOM", "生成 BOM")]
        public void GenerateBOM()
        {
            try
            {
                Logger.Info("启动 BOM 生成命令");
                
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null)
                {
                    Application.ShowAlertDialog("没有活动的文档");
                    return;
                }

                var editor = doc.Editor;
                editor.WriteMessage("\n正在分析装配体结构...");

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    // 提取 BOM 数据
                    var bomData = _bomGenerator.ExtractBOM(doc.Database, trans);
                    
                    if (bomData.Items.Count == 0)
                    {
                        editor.WriteMessage("\n⚠ 未找到任何零件");
                        return;
                    }

                    editor.WriteMessage($"\n✓ 找到 {bomData.Items.Count} 个零件");

                    // 导出到 Excel
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                        FileName = $"BOM_{System.DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                        Title = "保存 BOM 表"
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _excelExporter.ExportBOM(bomData, saveDialog.FileName);
                        editor.WriteMessage($"\n✓ BOM 表已导出到：{saveDialog.FileName}");
                        Logger.Info($"BOM 表已导出：{saveDialog.FileName}");
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BOM 生成失败", ex);
                Application.ShowAlertDialog($"BOM 生成失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 生成带重量的 BOM 表
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("GENBOMWEIGHT", "生成重量 BOM")]
        public void GenerateBOMWithWeight()
        {
            try
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;

                var editor = doc.Editor;
                editor.WriteMessage("\n正在计算零件重量...");

                using (var trans = doc.Database.TransactionManager.StartTransaction())
                {
                    var bomData = _bomGenerator.ExtractBOMWithWeight(doc.Database, trans);
                    
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                        FileName = $"BOM_Weight_{System.DateTime.Now:yyyyMMdd_HHmmss}.xlsx",
                        Title = "保存 BOM 表"
                    };

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        _excelExporter.ExportBOM(bomData, saveDialog.FileName);
                        editor.WriteMessage($"\n✓ 带重量的 BOM 表已导出");
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("重量 BOM 生成失败", ex);
                Application.ShowAlertDialog($"操作失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 从 Excel 导入 BOM 数据
        /// </summary>
        [Autodesk.AutoCAD.Runtime.Command("IMPORTBOM", "导入 BOM")]
        public void ImportBOM()
        {
            try
            {
                var openDialog = new OpenFileDialog
                {
                    Filter = "Excel 文件 (*.xlsx)|*.xlsx",
                    Title = "选择 BOM 文件"
                };

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    var bomData = _excelExporter.ImportBOM(openDialog.FileName);
                    Application.ShowAlertDialog($"成功导入 {bomData.Items.Count} 条记录");
                }
            }
            catch (Exception ex)
            {
                Logger.Error("BOM 导入失败", ex);
                Application.ShowAlertDialog($"导入失败：{ex.Message}");
            }
        }
    }
}

#endif
