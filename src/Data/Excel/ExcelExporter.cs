using OfficeOpenXml;
using CadAutomationPlugin.Core.BOM;
using Shared.Logging;

namespace CadAutomationPlugin.Data.Excel
{
    /// <summary>
    /// Excel 导出器 - BOM 表导出
    /// </summary>
    public class ExcelExporter
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(ExcelExporter));

        static ExcelExporter()
        {
            // 设置 EPPlus 许可证
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// 导出 BOM 表到 Excel
        /// </summary>
        public void ExportBOM(BOMData bomData, string filePath)
        {
            Logger.Info($"导出 BOM 表到：{filePath}");

            try
            {
                using (var package = new ExcelPackage())
                {
                    // 添加工作表
                    var worksheet = package.Workbook.Worksheets.Add("BOM 表");

                    // 设置表头
                    SetupBOMHeaders(worksheet);

                    // 填充数据
                    FillBOMData(worksheet, bomData);

                    // 格式化
                    FormatBOMTable(worksheet, bomData.Items.Count);

                    // 保存
                    package.SaveAs(new FileInfo(filePath));
                }

                Logger.Info("BOM 表导出完成");
            }
            catch (Exception ex)
            {
                Logger.Error("BOM 表导出失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 导入 BOM 表
        /// </summary>
        public BOMData ImportBOM(string filePath)
        {
            Logger.Info($"导入 BOM 表：{filePath}");

            var bomData = new BOMData();

            try
            {
                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    
                    // 读取数据
                    int row = 2; // 从第 2 行开始（第 1 行是表头）
                    while (worksheet.Cells[row, 1].Value != null)
                    {
                        var item = new BOMItem
                        {
                            Index = row - 1,
                            PartNumber = worksheet.Cells[row, 1].Value?.ToString() ?? "",
                            Name = worksheet.Cells[row, 2].Value?.ToString() ?? "",
                            Quantity = (int)(worksheet.Cells[row, 3].Value ?? 1),
                            Material = worksheet.Cells[row, 4].Value?.ToString() ?? "",
                            UnitWeight = worksheet.Cells[row, 5].Value != null ? 
                                Convert.ToDouble(worksheet.Cells[row, 5].Value) : 0,
                            TotalWeight = worksheet.Cells[row, 6].Value != null ? 
                                Convert.ToDouble(worksheet.Cells[row, 6].Value) : 0,
                            Vendor = worksheet.Cells[row, 7].Value?.ToString() ?? "",
                            Remarks = worksheet.Cells[row, 8].Value?.ToString() ?? ""
                        };

                        bomData.Items.Add(item);
                        row++;
                    }
                }

                Logger.Info($"成功导入 {bomData.Items.Count} 条记录");
                return bomData;
            }
            catch (Exception ex)
            {
                Logger.Error("BOM 表导入失败", ex);
                throw;
            }
        }

        /// <summary>
        /// 设置表头
        /// </summary>
        private void SetupBOMHeaders(ExcelWorksheet worksheet)
        {
            // 表头
            worksheet.Cells[1, 1].Value = "序号";
            worksheet.Cells[1, 2].Value = "图号";
            worksheet.Cells[1, 3].Value = "名称";
            worksheet.Cells[1, 4].Value = "数量";
            worksheet.Cells[1, 5].Value = "材料";
            worksheet.Cells[1, 6].Value = "单重 (kg)";
            worksheet.Cells[1, 7].Value = "总重 (kg)";
            worksheet.Cells[1, 8].Value = "供应商";
            worksheet.Cells[1, 9].Value = "备注";

            // 表头样式
            using (var range = worksheet.Cells[1, 1, 1, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }
        }

        /// <summary>
        /// 填充 BOM 数据
        /// </summary>
        private void FillBOMData(ExcelWorksheet worksheet, BOMData bomData)
        {
            int row = 2;
            foreach (var item in bomData.Items)
            {
                worksheet.Cells[row, 1].Value = item.Index;
                worksheet.Cells[row, 2].Value = item.PartNumber;
                worksheet.Cells[row, 3].Value = item.Name;
                worksheet.Cells[row, 4].Value = item.Quantity;
                worksheet.Cells[row, 5].Value = item.Material;
                worksheet.Cells[row, 6].Value = item.UnitWeight;
                worksheet.Cells[row, 7].Value = item.TotalWeight;
                worksheet.Cells[row, 8].Value = item.Vendor;
                worksheet.Cells[row, 9].Value = item.Remarks;

                row++;
            }
        }

        /// <summary>
        /// 格式化 BOM 表
        /// </summary>
        private void FormatBOMTable(ExcelWorksheet worksheet, int rowCount)
        {
            // 自动调整列宽
            worksheet.Cells[1, 1, rowCount + 1, 9].AutoFitColumns();

            // 设置边框
            using (var range = worksheet.Cells[1, 1, rowCount + 1, 9])
            {
                range.Style.Border.Top.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Left.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                range.Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
            }

            // 数字格式
            using (var range = worksheet.Cells[2, 6, rowCount + 1, 7])
            {
                range.Style.Numberformat.Format = "0.000";
            }

            // 数量列居中
            using (var range = worksheet.Cells[2, 4, rowCount + 1, 4])
            {
                range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            }

            // 添加汇总行
            int totalRow = rowCount + 2;
            worksheet.Cells[totalRow, 1].Value = "合计";
            worksheet.Cells[totalRow, 4].Formula = $"SUM(D2:D{rowCount + 1})";
            worksheet.Cells[totalRow, 7].Formula = $"SUM(G2:G{rowCount + 1})";

            using (var range = worksheet.Cells[totalRow, 1, totalRow, 9])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightYellow);
            }
        }
    }
}
