#!/usr/bin/env pwsh
# 云编译准备脚本
# 用于在 GitHub Actions 中准备 AutoCAD 引用程序集

param(
    [string]$RefsDir = "refs",
    [switch]$SkipAutoCADRefs
)

$ErrorActionPreference = "Stop"

Write-Host "=== 云编译准备脚本 ===" -ForegroundColor Cyan

# 创建 refs 目录
if (!(Test-Path $RefsDir)) {
    Write-Host "创建 refs 目录..." -ForegroundColor Yellow
    New-Item -ItemType Directory -Path $RefsDir | Out-Null
}

# 如果跳过 AutoCAD 引用，创建空标识文件
if ($SkipAutoCADRefs) {
    Write-Host "跳过 AutoCAD 引用（使用条件编译）" -ForegroundColor Yellow
    Set-Content -Path "$RefsDir/.skiprefs" -Value "AutoCAD refs skipped for cloud build"
    exit 0
}

# 检查是否存在 refs 文件
$requiredDlls = @("AcMgd.dll", "AcDbMgd.dll", "AcCoreMgd.dll", "AdWindows.dll")
$missingDlls = @()

foreach ($dll in $requiredDlls) {
    $path = Join-Path $RefsDir $dll
    if (!(Test-Path $path)) {
        $missingDlls += $dll
    }
}

if ($missingDlls.Count -gt 0) {
    Write-Host "警告：缺少以下 AutoCAD 引用程序集：" -ForegroundColor Yellow
    $missingDlls | ForEach-Object { Write-Host "  - $_" }
    
    Write-Host "`n解决方案：" -ForegroundColor Cyan
    Write-Host "1. 从本地 AutoCAD 安装目录复制 DLL 到 refs/ 文件夹" -ForegroundColor White
    Write-Host "   源目录：C:\Program Files\Autodesk\AutoCAD 2024\" -ForegroundColor Gray
    Write-Host "   目标：refs/" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. 或在 GitHub Actions 中使用条件编译跳过 AutoCAD 引用" -ForegroundColor White
    Write-Host "   设置环境变量：SKIP_AUTOCAD_REFS=true" -ForegroundColor Gray
    
    # 创建条件编译标识
    Set-Content -Path "$RefsDir/.skiprefs" -Value "AutoCAD refs not available, using conditional compilation"
    Write-Host "`n已启用条件编译模式" -ForegroundColor Green
} else {
    Write-Host "所有 AutoCAD 引用程序集已就绪" -ForegroundColor Green
}

Write-Host "`n准备完成！" -ForegroundColor Green
