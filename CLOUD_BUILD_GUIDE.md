# 云编译配置指南

本文档说明如何在 GitHub Actions 中编译 CAD Automation Plugin。

## 📋 配置概览

### 文件结构

```
CadAutomationPlugin/
├── .github/workflows/
│   └── build.yml          # GitHub Actions 工作流
├── refs/                   # AutoCAD 引用程序集（可选）
├── src/
│   ├── CadAutomationPlugin/
│   ├── Core/
│   ├── Data/
│   ├── Shared/
│   └── UI/
├── Directory.Build.props   # 全局构建属性
├── prepare-cloud-build.ps1 # 云编译准备脚本
└── .gitignore
```

## 🔧 工作原理

### 本地编译
- 使用 `$(AutoCADPath)` 环境变量定位 AutoCAD DLL
- 需要安装 AutoCAD 2020+

### 云编译（GitHub Actions）
- 自动检测 `GITHUB_ACTIONS=true` 环境变量
- 使用条件编译跳过 AutoCAD 引用
- 或使用 `refs/` 目录中的引用程序集

## 📦 自动编译流程

### 触发条件

| 事件 | 触发分支 | 说明 |
|------|---------|------|
| `git push` | main, develop | 自动编译 |
| Pull Request | main | 自动编译 + 代码分析 |
| 手动触发 | 任意 | 在 Actions 页面点击 "Run workflow" |

### 编译步骤

1. **检出代码** - 从 GitHub 获取最新代码
2. **设置 .NET 6.0** - 安装 SDK
3. **准备环境** - 运行 `prepare-cloud-build.ps1`
4. **还原依赖** - `dotnet restore`
5. **编译** - `dotnet build --configuration Release`
6. **测试** - `dotnet test`（失败不阻塞）
7. **发布** - `dotnet publish`
8. **打包** - 创建 ZIP 安装包
9. **上传产物** - 保存 30 天

## 📥 下载构建产物

编译完成后（约 3-5 分钟）：

1. 访问：https://github.com/einfallen/CadAutomationPlugin/actions
2. 点击最新的构建记录（绿色 ✓ 表示成功）
3. 在页面底部 "Artifacts" 区域下载：
   - **CadAutomationPlugin-DLLs** - 所有编译输出的 DLL
   - **CadAutomationPlugin-Installer** - ZIP 安装包

## 🔐 安全说明

### AutoCAD 引用程序集

AutoCAD DLL 是 Autodesk 的专有文件，**不建议**提交到 Git 仓库。

**推荐方案**（已实现）：
- ✅ 使用条件编译，云编译时跳过 AutoCAD 引用
- ✅ 代码中包含 `#if CLOUD_BUILD` 条件编译指令

**替代方案**（如需完整编译）：
- 从本地 AutoCAD 安装目录复制 DLL 到 `refs/` 文件夹
- 将 `refs/*.dll` 添加到 `.gitignore`
- 在 GitHub Actions 中通过 Secrets 或外部存储获取

## 🛠️ 本地测试云编译配置

在本地模拟云编译环境：

```powershell
# PowerShell
$env:GITHUB_ACTIONS = "true"
dotnet build --configuration Release
```

或设置环境变量后编译：

```bash
# Git Bash
export GITHUB_ACTIONS=true
dotnet build --configuration Release
```

## 📊 构建状态徽章

将以下代码添加到 README.md 显示构建状态：

```markdown
[![Build Status](https://github.com/einfallen/CadAutomationPlugin/actions/workflows/build.yml/badge.svg)](https://github.com/einfallen/CadAutomationPlugin/actions/workflows/build.yml)
```

## ⚙️ 自定义配置

### 修改 .NET 版本

编辑 `.github/workflows/build.yml`：

```yaml
env:
  DOTNET_VERSION: '8.0.x'  # 改为 .NET 8.0
```

### 修改编译配置

```yaml
env:
  BUILD_CONFIGURATION: 'Debug'  # 或 'Release'
```

### 添加自动发布

在 `build.yml` 末尾添加：

```yaml
  release:
    needs: build-windows
    if: startsWith(github.ref, 'refs/tags/')
    runs-on: windows-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Download artifacts
      uses: actions/download-artifact@v3
      with:
        name: CadAutomationPlugin-Installer
    
    - name: Create Release
      uses: softprops/action-gh-release@v1
      with:
        files: CadAutomationPlugin.zip
      env:
        GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

## 🐛 故障排除

### 编译失败：找不到 AutoCAD 引用

**错误信息**：
```
error CS0006: Metadata file 'AcMgd.dll' could not be found
```

**解决方案**：
1. 确认 `Directory.Build.props` 存在
2. 检查 `GITHUB_ACTIONS` 环境变量是否正确设置
3. 查看构建日志中的 "准备云编译环境" 步骤

### 测试失败

测试失败不会阻塞构建。查看详细日志：
1. 点击构建记录
2. 展开 "运行测试" 步骤
3. 查看错误信息

### 产物下载失败

产物保留 30 天后自动删除。如需长期保存：
- 创建 GitHub Release
- 或配置外部存储（Azure Blob、S3 等）

## 📞 需要帮助？

- GitHub Actions 文档：https://docs.github.com/en/actions
- .NET 文档：https://docs.microsoft.com/dotnet
- 项目 Issues：https://github.com/einfallen/CadAutomationPlugin/issues

---

**最后更新**: 2026-03-12  
**适用版本**: GitHub Actions
