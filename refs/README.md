# AutoCAD 参考程序集存根
# 用于云编译（GitHub Actions）

在云编译环境中，我们没有 AutoCAD 安装，因此需要这些存根程序集来通过编译。

## 使用方法

### 方案 1：使用 NuGet 包（推荐）

AutoCAD .NET API 可以通过以下方式获取：

1. **Autodesk.AutoCAD.Runtime** (部分 API)
   ```
   dotnet add package Autodesk.AutoCAD.Runtime
   ```

2. **AutoCAD.NET** (官方包，需要 Autodesk 账号)
   ```
   dotnet add package AutoCAD.NET --version 2024.0.0
   ```

### 方案 2：创建虚拟存根（当前使用）

对于云编译，我们使用条件编译跳过 AutoCAD 特定的代码：

```csharp
#if !CLOUD_BUILD
// AutoCAD 特定代码
using Autodesk.AutoCAD.DatabaseServices;
#endif
```

### 方案 3：使用引用程序集

从本地 AutoCAD 安装复制 DLL 到 `refs/` 目录：

```bash
# 从本地 AutoCAD 安装目录复制
copy "C:\Program Files\Autodesk\AutoCAD 2024\AcMgd.dll" refs/
copy "C:\Program Files\Autodesk\AutoCAD 2024\AcDbMgd.dll" refs/
copy "C:\Program Files\Autodesk\AutoCAD 2024\AcCoreMgd.dll" refs/
copy "C:\Program Files\Autodesk\AutoCAD 2024\AdWindows.dll" refs/
```

**注意**: AutoCAD DLL 是专有文件，不应提交到 Git 仓库。

## 当前配置

项目使用 `Directory.Build.props` 检测云编译环境：

```xml
<CloudBuild Condition="'$(GITHUB_ACTIONS)' == 'true'">true</CloudBuild>
```

云编译时，条件编译常量 `CLOUD_BUILD` 会被定义。

## 编译错误解决

如果遇到 "找不到 AcMgd.dll" 错误：

### 本地编译
```bash
# 确保设置了 AutoCAD 路径
$env:AutoCADPath = "C:\Program Files\Autodesk\AutoCAD 2024"
dotnet build
```

### 云编译
确保 `prepare-cloud-build.ps1` 脚本运行，它会创建 `.skiprefs` 文件启用条件编译。

---

**最后更新**: 2026-03-12
