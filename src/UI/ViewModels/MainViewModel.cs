using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CadAutomationPlugin.Core.BOM;

namespace CadAutomationPlugin.UI.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        [ObservableProperty]
        private string statusMessage = "就绪";

        [ObservableProperty]
        private string versionInfo = "v1.0.0";

        [ObservableProperty]
        private ObservableCollection<BOMItem> bomItems = new();

        [ObservableProperty]
        private string selectedDimensionStyle = "Standard";

        [ObservableProperty]
        private string dimensionPrecision = "0.00";

        [ObservableProperty]
        private bool autoAvoidOverlap = true;

        [ObservableProperty]
        private double kFactor = 0.5;

        [ObservableProperty]
        private double sheetThickness = 2.0;

        [RelayCommand]
        private void SmartDimension()
        {
            StatusMessage = "执行智能标注...";
            // 调用 AutoCAD 命令
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("SMARTDIM ", true, false, false);
        }

        [RelayCommand]
        private void DimensionHoles()
        {
            StatusMessage = "标注孔特征...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("SMARTDIMHOLES ", true, false, false);
        }

        [RelayCommand]
        private void DimensionChamfer()
        {
            StatusMessage = "标注倒角...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("SMARTDIMCHAMFER ", true, false, false);
        }

        [RelayCommand]
        private void ClearDimensions()
        {
            StatusMessage = "清除标注...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("CLEARALLDIMS ", true, false, false);
        }

        [RelayCommand]
        private void GenerateBOM()
        {
            StatusMessage = "生成 BOM 表...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("GENBOM ", true, false, false);
        }

        [RelayCommand]
        private void GenerateBOMWeight()
        {
            StatusMessage = "生成带重量的 BOM 表...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("GENBOMWEIGHT ", true, false, false);
        }

        [RelayCommand]
        private void ImportBOM()
        {
            StatusMessage = "导入 BOM...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("IMPORTBOM ", true, false, false);
        }

        [RelayCommand]
        private void ExportExcel()
        {
            StatusMessage = "导出 Excel...";
        }

        [RelayCommand]
        private void AnalyzeDependencies()
        {
            StatusMessage = "分析关联关系...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("SHOWDEPENDENCIES ", true, false, false);
        }

        [RelayCommand]
        private void BatchChange()
        {
            StatusMessage = "执行批量修改...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("BATCHCHANGE ", true, false, false);
        }

        [RelayCommand]
        private void RegisterLink()
        {
            StatusMessage = "注册关联...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("REGISTERLINK ", true, false, false);
        }

        [RelayCommand]
        private void ClearLinks()
        {
            StatusMessage = "清除关联...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("CLEARLINKS ", true, false, false);
        }

        [RelayCommand]
        private void GenerateUnfold()
        {
            StatusMessage = "生成展开图...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("GENUNFOLD ", true, false, false);
        }

        [RelayCommand]
        private void BatchUnfold()
        {
            StatusMessage = "批量展开...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("BATCHUNFOLD ", true, false, false);
        }

        [RelayCommand]
        private void GenerateDrawing()
        {
            StatusMessage = "参数化出图...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("GENDRAWING ", true, false, false);
        }

        [RelayCommand]
        private void GenerateProcess()
        {
            StatusMessage = "生成工艺文件...";
            Autodesk.AutoCAD.ApplicationServices.Application.DocumentManager.MdiActiveDocument?
                .SendStringToExecute("GENPROCESS ", true, false, false);
        }

        [RelayCommand]
        private void ManageTemplates()
        {
            StatusMessage = "管理模板...";
        }
    }
}
