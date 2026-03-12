# 云编译故障排除

本文档帮助解决 GitHub Actions 云编译中的常见问题。

---

## 🔍 常见错误及解决方案

### 错误 1: 找不到 AutoCAD 引用 (AcMgd.dll)

**错误信息**:
```
error CS0006: Metadata file 'AcMgd.dll' could not be found
```

**原因**: 
- 云编译环境没有安装 AutoCAD
- `refs/` 目录为空

**解决方案**:

#### ✅ 方案 A: 使用条件编译（推荐）

确保 `Directory.Build.props` 正确检测云编译：

```xml
<CloudBuild Condition="'$(GITHUB_ACTIONS)' == 'true'">true</CloudBuild>
```

项目文件中使用条件引用：

```xml
<ItemGroup Condition="'$(CloudBuild)' != 'true'">
  <Reference Include="AcMgd">
    <HintPath>$(AutoCADPath)\AcMgd.dll</HintPath>
  </Reference>
</ItemGroup>
```

#### 方案 B: 提供 AutoCAD 引用程序集

从本地 AutoCAD 安装复制 DLL 到 `refs/` 目录：

```bash
copy "C:\Program Files\Autodesk\AutoCAD 2024\AcMgd.dll" refs/
```

**注意**: 不要将 AutoCAD DLL 提交到 Git（已添加到 `.gitignore`）

---

### 错误 2: CLOUD_BUILD 环境变量未设置

**错误信息**:
```
The name 'CLOUD_BUILD' does not exist in the current context
```

**原因**: 
- GitHub Actions 工作流未设置环境变量

**解决方案**:

在 `.github/workflows/build.yml` 中设置：

```yaml
env:
  CLOUD_BUILD: 'true'
```

确保每个需要条件编译的步骤都有：

```yaml
- name: 编译
  run: dotnet build
  env:
    CLOUD_BUILD: true
```

---

### 错误 3: 条件编译未生效

**错误信息**:
```
namespace 'Autodesk.AutoCAD.DatabaseServices' not found
```

**原因**:
- 代码中未使用 `#if !CLOUD_BUILD` 条件编译
- 或 `Directory.Build.props` 未定义 `CLOUD_BUILD` 常量

**解决方案**:

#### 检查 Directory.Build.props

确保包含：

```xml
<PropertyGroup Condition="'$(CloudBuild)' == 'true'">
  <DefineConstants>$(DefineConstants);CLOUD_BUILD</DefineConstants>
</PropertyGroup>
```

#### 检查代码

使用条件编译：

```csharp
#if !CLOUD_BUILD
using Autodesk.AutoCAD.DatabaseServices;

public void AutoDimension(Database db, ...)
{
    // AutoCAD 特定代码
}
#endif
```

或使用 `#if CLOUD_BUILD` 提供替代实现：

```csharp
#if CLOUD_BUILD
// 云编译存根实现
public void AutoDimension(object db, object selection, object trans)
{
    // 空实现或抛出 NotImplementedException
}
#else
// 完整实现
public void AutoDimension(Database db, ...)
{
    // 实际代码
}
#endif
```

---

### 错误 4: 编译成功但测试失败

**错误信息**:
```
Test run failed: No test assemblies found
```

**原因**:
- 测试项目引用了 AutoCAD 特定的代码
- 云编译时测试项目未正确配置

**解决方案**:

#### 方案 A: 跳过测试（临时）

在 `build.yml` 中设置：

```yaml
- name: 运行测试
  run: dotnet test
  continue-on-error: true
```

#### 方案 B: 配置测试项目

修改测试项目 `.csproj`：

```xml
<PropertyGroup Condition="'$(CloudBuild)' == 'true'">
  <DefineConstants>$(DefineConstants);CLOUD_BUILD</DefineConstants>
</PropertyGroup>
```

测试代码中使用条件编译：

```csharp
#if !CLOUD_BUILD
[Fact]
public void AutoCAD_Integration_Test()
{
    // 需要 AutoCAD 的测试
}
#endif
```

---

### 错误 5: 构建时间过长

**现象**: 编译超过 10 分钟

**原因**:
- 还原依赖慢
- 编译优化未启用

**解决方案**:

#### 启用缓存

在 `build.yml` 中添加：

```yaml
- name: 缓存 NuGet 包
  uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/*.csproj') }}
    restore-keys: |
      ${{ runner.os }}-nuget-
```

#### 优化编译选项

```yaml
- name: 编译
  run: dotnet build --configuration Release --no-restore -m:1
```

`-m:1` 限制并行编译，减少内存使用。

---

## 📊 调试步骤

### 1. 启用详细日志

在 `build.yml` 中：

```yaml
- name: 编译
  run: dotnet build --verbosity diagnostic
```

### 2. 检查环境变量

添加调试步骤：

```yaml
- name: 调试环境变量
  shell: pwsh
  run: |
    Write-Host "CLOUD_BUILD = $env:CLOUD_BUILD"
    Write-Host "GITHUB_ACTIONS = $env:GITHUB_ACTIONS"
    Write-Host "DOTNET_VERSION = $env:DOTNET_VERSION"
```

### 3. 查看完整构建日志

1. 访问：https://github.com/einfallen/CadAutomationPlugin/actions
2. 点击失败的构建
3. 展开 `build-windows` 任务
4. 点击各个步骤查看详细日志

### 4. 本地重现

在本地模拟云编译环境：

```powershell
# PowerShell
$env:CLOUD_BUILD = "true"
$env:GITHUB_ACTIONS = "true"
dotnet build --configuration Release --verbosity diagnostic
```

---

## 🛠️ 验证清单

编译前确认：

- [ ] `Directory.Build.props` 存在且正确配置
- [ ] `.csproj` 文件使用条件引用
- [ ] `.github/workflows/build.yml` 设置 `CLOUD_BUILD=true`
- [ ] `prepare-cloud-build.ps1` 脚本存在
- [ ] `.gitignore` 排除 `refs/*.dll`

---

## 📞 需要帮助？

### 相关文档

- [CLOUD_BUILD_GUIDE.md](CLOUD_BUILD_GUIDE.md) - 云编译配置指南
- [RELEASE_GUIDE.md](RELEASE_GUIDE.md) - 自动发布指南
- [CODE_QUALITY_REPORT.md](CODE_QUALITY_REPORT.md) - 代码质量报告

### 外部资源

- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [.NET 编译任务](https://docs.microsoft.com/dotnet/core/tools/dotnet-build)
- [MSBuild 条件](https://docs.microsoft.com/visualstudio/msbuild/msbuild-conditions)

### 联系支持

- 提交 Issue: https://github.com/einfallen/CadAutomationPlugin/issues
- 讨论区：https://github.com/einfallen/CadAutomationPlugin/discussions

---

**最后更新**: 2026-03-12  
**适用版本**: GitHub Actions
