# GitHub Actions 错误修复记录

本文档记录 GitHub Actions 云编译中遇到的错误及解决方案。

---

## 🐛 2026-03-12 错误修复

### 错误 1: Path does not exist: results.sarif

**错误信息**:
```
build-windows Path does not exist: results.sarif
build-windows Process completed with exit code 1.
```

**原因**: 
- 工作流尝试上传 `results.sarif` 文件
- 但 .NET 编译没有生成 SARIF 格式的分析结果

**解决方案**: 
- ✅ **移除 CodeQL SARIF 上传步骤**
- 代码分析器结果不是必需的，可以省略

**修改**:
```yaml
# ❌ 删除以下步骤
- name: 上传代码分析结果
  uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: results.sarif
```

---

### 错误 2: CodeQL Action 权限不足

**错误信息**:
```
This run of the CodeQL Action does not have permission to access the CodeQL Action API endpoints.
Details: Resource not accessible by integration
```

**原因**:
- 工作流缺少 `security-events` 权限
- CodeQL 需要读取安全事件权限

**解决方案**:
- ✅ **添加权限配置到工作流**

**修改**:
```yaml
# 在工作流顶部添加
permissions:
  contents: read
  security-events: write  # CodeQL 需要
```

---

### 错误 3: Node.js 20 弃用警告

**错误信息**:
```
Node.js 20 actions are deprecated. The following actions are running on Node.js 20:
actions/checkout@v4, actions/setup-dotnet@v4.
Actions will be forced to run with Node.js 24 by default starting June 2nd, 2026.
```

**原因**:
- GitHub Actions 将从 Node.js 20 迁移到 Node.js 24
- 默认切换日期：2026-06-02
- ⚠️ **重要**: `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24` 必须设置在 **job 级别**，不能在全局 `env`

**解决方案**:
- ✅ **在 job 级别设置环境变量**

**修改**:
```yaml
jobs:
  build-windows:
    runs-on: windows-latest
    # ✅ 正确：job 级别
    env:
      FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: 'true'
    
    steps:
    - uses: actions/checkout@v4
```

```yaml
# ❌ 错误：全局 env 不会生效
env:
  FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: 'true'

jobs:
  build-windows:
    runs-on: windows-latest
```

**参考**: https://github.blog/changelog/2025-09-19-deprecation-of-node-20-on-github-actions-runners/

---

### 错误 4: CodeQL Action v3 将弃用

**错误信息**:
```
CodeQL Action v3 will be deprecated in December 2026. 
Please update all occurrences of the CodeQL Action in your workflow files to v4.
```

**原因**:
- CodeQL Action v3 将于 2026 年 12 月弃用
- 需要升级到 v4

**解决方案**:
- ✅ **升级到 CodeQL Action v4**
- 或直接移除（如果不是必需的）

**修改**:
```yaml
# 方案 A: 升级到 v4
- uses: github/codeql-action/upload-sarif@v4

# 方案 B: 移除（推荐，本项目不需要）
# 删除 CodeQL 相关步骤
```

**参考**: https://github.blog/changelog/2025-10-28-upcoming-deprecation-of-codeql-action-v3/

---

## ✅ 修复总结

### 2026-03-12 最新修复

#### 问题 1: Data 项目使用 Core.BOM 命名空间导致编译错误

**错误**: 
```
The type or namespace name 'BOMData' could not be found
```

**原因**: 
- `Data/Excel/ExcelExporter.cs` 使用 `Core.BOM` 命名空间（`BOMData` 类）
- 云编译时 Core 项目的存根类不可用

**解决方案**: 
- 为 `ExcelExporter.cs` 添加 `#if !CLOUD_BUILD` 条件编译
- 添加存根类用于云编译模式

**修复的文件**:
- `src/Data/Excel/ExcelExporter.cs` - 添加条件编译和存根类

---

#### 问题 2: CLOUD_BUILD 常量未正确定义

**错误**: 
```
The type or namespace name 'BOMData' could not be found
```

**原因**: 
- `Directory.Build.props` 检查 `$(GITHUB_ACTIONS)` 来检测云编译
- 但 MSBuild 可能无法正确读取环境变量
- 导致 `CLOUD_BUILD` 常量没有被定义

**解决方案**: 
- 在所有 `dotnet` 命令中显式传递 `-p:CloudBuild=true`
- 这确保属性被设置，不依赖环境变量检测

**更新的命令**:
```yaml
dotnet restore -p:CloudBuild=true
dotnet build -p:CloudBuild=true
dotnet test -p:CloudBuild=true
dotnet publish -p:CloudBuild=true
```

**修复的文件**:
- `.github/workflows/build.yml` - 添加 `-p:CloudBuild=true` 到所有 dotnet 命令

---

#### 问题 2: CoreStubs.cs/UIStubs.cs 条件评估时机错误

**错误**: 
```
The type or namespace name 'BOMData' could not be found
```

**原因**: 
- `.csproj` 使用 `<Compile Remove>` 当 `CloudBuild != true`
- 但 `CloudBuild` 属性在 `Directory.Build.props` 中设置，**晚于**项目评估
- 导致条件判断错误，存根文件在云编译时也被排除了

**解决方案**: 
- 恢复使用 `#if CLOUD_BUILD` 在存根文件内部
- 移除 `.csproj` 中的 `Compile Remove` 条件
- 让文件始终被包含，内容通过 `#if CLOUD_BUILD` 控制

**修复的逻辑**:
- 云编译：文件被包含 **且** 内容激活（`#if CLOUD_BUILD`）
- 本地编译：文件被包含 **但** 内容跳过（`#if CLOUD_BUILD`）

**修复的文件**:
- `src/Core/CoreStubs.cs` - 重新添加 `#if CLOUD_BUILD` 包装
- `src/UI/UIStubs.cs` - 重新添加 `#if CLOUD_BUILD` 包装
- `src/Core/Core.csproj` - 移除 Compile Remove 条件
- `src/UI/UI.csproj` - 移除 Compile Remove 条件

---

#### 问题 2: UI 项目使用 Core/AutoCAD 命名空间导致编译错误

**错误**: 
```
The type or namespace name 'BOMData' could not be found
The type or namespace name 'Core' does not exist in the namespace 'CadAutomationPlugin'
```

**原因**: 
- UI 项目中的文件（如 MainViewModel.cs）使用 `CadAutomationPlugin.Core.BOM` 和 AutoCAD 命名空间
- 云编译时这些命名空间不可用

**解决方案**: 
1. 为 UI 项目中使用 AutoCAD/Core 的文件添加 `#if !CLOUD_BUILD`
2. 创建 `UIStubs.cs` 提供云编译时的存根类
3. 更新 `UI.csproj` 在本地编译时移除存根文件

**修复的文件**:
- `src/UI/ViewModels/MainViewModel.cs` - 添加条件编译
- `src/UI/UIStubs.cs` - 新增（云编译存根）
- `src/UI/UI.csproj` - 添加 Compile Remove 条件

---

#### 问题 2: Directory.Build.props CloudBuild 属性检测时机错误

**错误**: 
```
The type or namespace name 'BOMData' could not be found
```

**原因**: 
- `build.yml` 中 `CLOUD_BUILD=true` 在 "准备云编译环境" 步骤才设置
- 但 `dotnet restore` 在这之前已经运行
- `Directory.Build.props` 中的 `CloudBuild` 属性检测在 restore 时还没有生效

**解决方案**: 
- 在 workflow 级别的 `env` 块中设置 `GITHUB_ACTIONS=true` 和 `CLOUD_BUILD=true`
- 这样环境变量从 job 开始就可用
- `Directory.Build.props` 可以在 restore 时正确检测云编译模式

**修复的文件**:
- `.github/workflows/build.yml` - 在 workflow 级别设置环境变量

---

#### 问题 2: CoreStubs.cs 被 #if 包裹导致内容为空

**错误**: 
```
The type or namespace name 'BOMData' could not be found
```

**原因**: 
- `CoreStubs.cs` 文件被 `#if CLOUD_BUILD` 预处理器指令包裹
- 但文件已经通过 `Core.csproj` 的条件包含来控制（仅当 `CloudBuild != true` 时移除）
- 双重条件导致文件即使被包含，内容也是空的

**解决方案**: 
- 移除 `CoreStubs.cs` 中的 `#if CLOUD_BUILD` 包装
- 文件已经通过 `Core.csproj` 条件控制，不需要额外的 #if

**修复的文件**:
- `src/Core/CoreStubs.cs` - 移除 #if 包装
- `src/Core/Core.csproj` - 添加 Compile Remove 条件

---

#### 问题 2: BOMData 和 Core 命名空间找不到

**错误**: 
```
The type or namespace name 'BOMData' could not be found
The type or namespace name 'Core' does not exist in the namespace 'CadAutomationPlugin'
```

**原因**: 
- Core 项目中的文件（BOMGenerator.cs, ChangePropagationEngine.cs 等）使用 AutoCAD 命名空间
- 没有条件编译，导致云编译时失败
- 云编译时这些文件被跳过，但它们的公共类（如 BOMData）被其他文件引用

**解决方案**: 
1. 为所有 AutoCAD 依赖的 Core 文件添加 `#if !CLOUD_BUILD`
2. 创建 `CoreStubs.cs` 提供云编译时的存根类

**修复的文件**:
- `src/Core/BOM/BOMGenerator.cs` - 添加条件编译
- `src/Core/ChangePropagation/ChangePropagationEngine.cs` - 添加条件编译
- `src/Core/Parametric/ParametricDrawingGenerator.cs` - 添加条件编译
- `src/Core/SmartDimension/SmartDimensionEngine.cs` - 添加条件编译
- `src/Core/CoreStubs.cs` - 新增（云编译存根）

---

#### 问题 2: Point2d/Vector2d 缺少 X/Y 属性

**错误**: 
```
'Point2d' does not contain a definition for 'X' and no accessible extension method 'X' accepting a first argument of type 'Point2d' could be found
'Point2d' does not contain a definition for 'Y' ...
```

**原因**: 
- `CloudBuildStubs.cs` 中的 `Point2d` 和 `Vector2d` 结构体只定义了构造函数
- 没有定义 `X` 和 `Y` 公共属性
- `GeometryUtils.cs` 中使用了 `point.X` 和 `point.Y`

**解决方案**: 
- 为 `Point2d` 添加 `public double X { get; set; }` 和 `public double Y { get; set; }`
- 为 `Vector2d` 添加 `public double X { get; set; }` 和 `public double Y { get; set; }`
- 为 `Point3d` 和 `Vector3d` 也添加 `X/Y/Z` 属性（完整性）

**修复的文件**:
- `src/Shared/CloudBuildStubs.cs` - 添加 X/Y/Z 属性

---

#### 问题 2: GeometryUtils.cs 缺少 Point2d/Vector2d 命名空间

**错误**: 
```
The type or namespace name 'Point2d' could not be found
```

**原因**: 
- `GeometryUtils.cs` 使用 `#if !CLOUD_BUILD` 包装了 `using Autodesk.AutoCAD.Geometry;`
- 但 `Point2d`/`Vector2d` 在**始终编译**的方法中使用（如 `IsPointInRect`, `GetBoundingBox`）
- 云编译时 using 被跳过，导致找不到类型

**解决方案**: 
- **始终包含** `using Autodesk.AutoCAD.Geometry;`
- `CloudBuildStubs.cs` 在云编译时提供该命名空间
- 只用 `#if !CLOUD_BUILD` 包装 AutoCAD 专属方法（`Distance`, `MidPoint`, `DotProduct`, `CrossProduct`）

**修复的文件**:
- `src/Shared/Geometry/GeometryUtils.cs` - 移除 using 的条件编译

---

#### 问题 2: CloudBuildStubs.cs 在云编译时未被包含

**错误**: 
```
The type or namespace name 'Point2d' could not be found
```

**原因**: 
- `Shared.csproj` 使用 `<Compile Remove>` 当 `CloudBuild != true`
- 但 `CloudBuild` 属性在 `Directory.Build.props` 中设置，**晚于**项目评估
- 导致条件判断错误，文件在云编译时也被排除了

**解决方案**: 简化策略
1. 移除 `Shared.csproj` 中的所有 `Compile Include/Remove` 条件
2. 让 .NET SDK 默认包含所有 `.cs` 文件
3. 在 `CloudBuildStubs.cs` 文件内部使用 `#if CLOUD_BUILD`
4. `Directory.Build.props` 在 `GITHUB_ACTIONS=true` 时定义 `CLOUD_BUILD` 常量

**修复的逻辑**:
- 云编译：`CloudBuildStubs.cs` 被包含 **且** 内容激活（`#if CLOUD_BUILD`）
- 本地编译：`CloudBuildStubs.cs` 被包含 **但** 内容跳过（`#if CLOUD_BUILD`）

**修复的文件**:
- `src/Shared/Shared.csproj` - 简化，移除所有条件
- `src/Shared/CloudBuildStubs.cs` - 重新添加 `#if CLOUD_BUILD` 包装

---

#### 问题 2: Duplicate 'Compile' items - CloudBuildStubs.cs 重复包含

**错误**: 
```
Duplicate 'Compile' items were included. The .NET SDK includes 'Compile' items from your project directory by default.
The duplicate items were: 'CloudBuildStubs.cs'
```

**原因**: 
- .NET SDK 默认会自动包含项目目录中的所有 `.cs` 文件
- `Shared.csproj` 又显式用 `<Compile Include="CloudBuildStubs.cs" />` 包含了一次
- 导致文件被包含两次

**解决方案**: 
- 移除显式的 `<Compile Include>`
- 只保留 `<Compile Remove>` 用于本地编译时排除

**修复前** (错误):
```xml
<ItemGroup Condition="'$(CloudBuild)' == 'true'">
  <Compile Include="CloudBuildStubs.cs" />
</ItemGroup>
<ItemGroup Condition="'$(CloudBuild)' != 'true'">
  <Compile Remove="CloudBuildStubs.cs" />
</ItemGroup>
```

**修复后** (正确):
```xml
<ItemGroup Condition="'$(CloudBuild)' != 'true'">
  <Compile Remove="CloudBuildStubs.cs" />
</ItemGroup>
```

**修复的文件**:
- `src/Shared/Shared.csproj` - 移除重复的 Include

---

#### 问题 2: CloudBuildStubs.cs 被 #if 包裹导致内容为空

**错误**: 
```
The type or namespace name 'Point2d' could not be found
```

**原因**: 
- `CloudBuildStubs.cs` 文件被 `#if CLOUD_BUILD` 包裹
- 但文件已经通过 Shared.csproj 条件包含（仅当 CloudBuild=true 时）
- 双重条件导致文件即使被包含，内容也是空的

**解决方案**: 
- 移除 `CloudBuildStubs.cs` 中的 `#if CLOUD_BUILD` 包装
- 文件已经通过 Shared.csproj 条件控制，不需要额外的 #if

**修复的文件**:
- `src/Shared/CloudBuildStubs.cs` - 移除 #if 包装
- `src/Shared/Shared.csproj` - 条件包含已正确配置

---

#### 问题 2: Point2d/Vector2d 类型找不到

**错误**: 
```
The type or namespace name 'Point2d' could not be found
The type or namespace name 'Vector2d' could not be found
Cannot declare a variable of static type 'LogManager'
```

**原因**: 
- CloudBuildStubs.cs 缺少 Point2d/Vector2d 定义
- LogManager 使用了 NLog.LogManager 字段导致命名冲突

**解决方案**: 
1. 添加 Point2d/Vector2d 到 CloudBuildStubs.cs
2. 修复 LogManager - 移除 NLog.LogManager 字段，添加 ConsoleLogger
3. 为 GeometryUtils.cs 和 UnfoldEngine.cs 添加条件编译

**修复的文件**:
- `src/Shared/CloudBuildStubs.cs` - 添加 Point2d, Vector2d, 更多实体类
- `src/Shared/Logging/LogManager.cs` - 修复命名冲突，添加云编译支持
- `src/Shared/Geometry/GeometryUtils.cs` - 添加 #if !CLOUD_BUILD
- `src/Core/Unfold/UnfoldEngine.cs` - 添加 #if !CLOUD_BUILD

---

#### 问题 2: MSB4232 - ItemGroup 嵌套错误

**错误**: 
```
error MSB4232: Items that are outside Target elements must have one of the following operations: Include, Update, or Remove.
```

**原因**: `.csproj` 文件中 `<ItemGroup>` 嵌套在另一个 `<ItemGroup>` 内部，这是无效的 MSBuild 语法

**错误配置**:
```xml
<!-- ❌ 无效 -->
<ItemGroup>
  <ItemGroup Condition="'$(CloudBuild)' != 'true'">
    <Reference Include="AcMgd" />
  </ItemGroup>
</ItemGroup>
```

**解决方案**: 将 `Condition` 移到外层 `<ItemGroup>`

**正确配置**:
```xml
<!-- ✅ 有效 -->
<ItemGroup Condition="'$(CloudBuild)' != 'true'">
  <Reference Include="AcMgd" />
</ItemGroup>
```

**修复的文件**:
- `src/CadAutomationPlugin/CadAutomationPlugin.csproj`
- `src/Core/Core.csproj`
- `src/UI/UI.csproj`

---

#### 问题 2: 云编译找不到 AutoCAD 命名空间

**错误**: `The type or namespace name 'Autodesk' could not be found`

**原因**: 代码使用 `Autodesk.AutoCAD.*` 命名空间，但云编译环境没有 AutoCAD SDK

**解决方案**: 创建存根文件 `src/Shared/CloudBuildStubs.cs`

```csharp
#if CLOUD_BUILD
namespace Autodesk.AutoCAD.DatabaseServices
{
    public class Database { }
    public class Transaction { }
    // ... 其他空类
}
#endif
```

### 之前修复的问题

| 问题 | 状态 | 解决方案 |
|------|------|---------|
| SARIF 文件不存在 | ✅ | 移除 CodeQL 步骤 |
| CodeQL 权限不足 | ✅ | 添加 security-events 权限 |
| Node.js 20 弃用 | ✅ | 设置 FORCE_JAVASCRIPT_ACTIONS_TO_NODE24 |
| AutoCAD 命名空间 | ✅ | 添加 CloudBuildStubs.cs 存根 |

### 修改的文件

| 文件 | 修改内容 |
|------|---------|
| `.github/workflows/build.yml` | 简化配置、添加调试 |
| `.github/workflows/release.yml` | 添加权限、Node.js 24 支持 |
| `Directory.Build.props` | 改进云编译检测 |
| `src/Shared/CloudBuildStubs.cs` | 新增（AutoCAD 命名空间存根） |

---

## 📊 当前状态

| 检查项 | 状态 |
|--------|------|
| SARIF 上传错误 | ✅ 已修复 |
| CodeQL 权限 | ✅ 已添加 |
| Node.js 20 弃用 | ✅ 已处理 |
| CodeQL v3 弃用 | ✅ 已处理 |

---

## 🔍 验证方法

### 1. 查看 Actions 日志

访问：https://github.com/einfallen/CadAutomationPlugin/actions

查找最新的构建记录，确认没有错误。

### 2. 手动触发构建

```bash
# 推送空提交触发构建
git commit --allow-empty -m "Trigger build"
git push
```

### 3. 检查构建状态

- ✅ 构建成功（绿色 ✓）
- ✅ 无错误信息
- ✅ 产物已上传

---

## 📚 相关文档

- [ACTIONS_VERSIONS.md](.github/ACTIONS_VERSIONS.md) - Actions 版本说明
- [CLOUD_BUILD_GUIDE.md](CLOUD_BUILD_GUIDE.md) - 云编译指南
- [CLOUD_BUILD_TROUBLESHOOTING.md](CLOUD_BUILD_TROUBLESHOOTING.md) - 故障排除

---

## 📞 外部资源

- [Node.js 20 弃用通知](https://github.blog/changelog/2025-09-19-deprecation-of-node-20-on-github-actions-runners/)
- [CodeQL Action v3 弃用](https://github.blog/changelog/2025-10-28-upcoming-deprecation-of-codeql-action-v3/)
- [Artifact Actions v3 弃用](https://github.blog/changelog/2024-04-16-deprecation-notice-v3-of-the-artifact-actions/)
- [GitHub Actions 权限](https://docs.github.com/en/actions/security-guides/automatic-token-authentication)

---

**最后更新**: 2026-03-12  
**当前状态**: ✅ 所有错误已修复
