# 🚀 Windows 编译指南

## 📦 下载项目

**压缩包位置**: `/home/admin/openclaw/workspace/CadAutomationPlugin.zip`

**文件大小**: 47KB

**下载方式**:
- **WinSCP**: 连接服务器后下载该文件
- **FileZilla**: SFTP 协议，下载到本地
- **SCP 命令**: 
  ```powershell
  scp admin@your-server:/home/admin/openclaw/workspace/CadAutomationPlugin.zip C:\Downloads\
  ```

---

## 📋 Windows 端操作步骤

### 步骤 1: 安装 .NET 6.0 SDK

1. 访问：https://dotnet.microsoft.com/download/dotnet/6.0
2. 下载 **SDK x64** 安装包
3. 双击运行安装程序
4. 安装完成后，打开命令提示符验证：
   ```powershell
   dotnet --version
   # 应显示 6.0.xxx
   ```

### 步骤 2: 解压项目

```powershell
# 解压到任意位置，例如：
C:\Projects\CadAutomationPlugin\
```

### 步骤 3: 配置 AutoCAD 路径

**方法 A: 设置环境变量**（推荐）

1. 右键"此电脑" → 属性 → 高级系统设置
2. 点击"环境变量"
3. 在"系统变量"中新建：
   - 变量名：`AutoCADPath`
   - 变量值：`C:\Program Files\Autodesk\AutoCAD 2024`
   （根据你的实际安装路径调整）

**方法 B: 修改项目文件**

编辑 `src\CadAutomationPlugin\CadAutomationPlugin.csproj`，修改路径：
```xml
<Reference Include="AcMgd">
  <HintPath>C:\Program Files\Autodesk\AutoCAD 2024\AcMgd.dll</HintPath>
</Reference>
```

### 步骤 4: 还原依赖

```powershell
cd C:\Projects\CadAutomationPlugin
dotnet restore
```

**预期输出**:
```
  Determining projects to restore...
  Restored Shared.csproj (in 2.1 sec).
  Restored Data.csproj (in 2.1 sec).
  Restored Core.csproj (in 2.1 sec).
  Restored UI.csproj (in 2.1 sec).
  Restored CadAutomationPlugin.csproj (in 2.1 sec).
  Restore succeeded.
```

### 步骤 5: 编译项目

```powershell
# 调试版本
dotnet build

# 发布版本（推荐）
dotnet publish --configuration Release
```

**预期输出**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

**输出位置**:
```
C:\Projects\CadAutomationPlugin\src\CadAutomationPlugin\bin\Release\net6.0-windows\
```

### 步骤 6: 在 AutoCAD 中加载

1. **启动 AutoCAD** (2020 或更高版本)

2. **输入命令**: `NETLOAD`

3. **浏览到 DLL 文件**:
   ```
   C:\Projects\CadAutomationPlugin\src\CadAutomationPlugin\bin\Release\net6.0-windows\CadAutomationPlugin.dll
   ```

4. **点击"打开"**

5. **验证加载成功**:
   - 命令行应显示：`✓ CAD 自动化插件已加载 (输入 CADAUTO 启动)`

### 步骤 7: 测试功能

在 AutoCAD 命令行输入以下命令测试：

```
SMARTDIM       # 智能标注
GENBOM         # 生成 BOM 表
BATCHCHANGE    # 批量改图
SHOWDEPENDENCIES  # 查看关联
GENUNFOLD      # 展开图
```

---

## 🐛 常见问题解决

### 问题 1: `dotnet` 不是内部命令

**解决**:
- 重启命令提示符（管理员）
- 或添加 .NET 到 PATH: `C:\Program Files\dotnet\`

### 问题 2: 找不到 AutoCAD 引用

**错误**: `The referenced component 'AcMgd' could not be found.`

**解决**:
```powershell
# 检查 AutoCAD 是否安装
dir "C:\Program Files\Autodesk\AutoCAD*"

# 确认环境变量
echo %AutoCADPath%
```

### 问题 3: 编译时提示缺少 Windows 桌面 SDK

**错误**: `The current .NET SDK does not include targeting pack net6.0-windows.`

**解决**:
- 重新运行 .NET 6.0 SDK 安装程序
- 确保勾选了 ".NET 桌面开发" 组件

### 问题 4: AutoCAD 加载 DLL 失败

**错误**: `Could not load file or assembly`

**解决**:
1. 确认 AutoCAD 版本与 .NET 版本兼容
2. 复制所有 DLL 到 AutoCAD 支持路径
3. 或以管理员身份运行 AutoCAD

### 问题 5: 命令不响应

**解决**:
- 检查命令行输入是否正确（不区分大小写）
- 输入 `CADAUTO` 打开主窗口
- 查看 AutoCAD 命令行是否有错误信息

---

## 📊 项目文件清单

解压后应包含以下文件：

```
✅ CadAutomationPlugin.sln              # Visual Studio 解决方案
✅ README.md                            # 项目说明
✅ BUILD.md                             # 构建说明
✅ BUILD_REPORT.md                      # 测试报告
✅ QUICKSTART.md                        # 快速开始
✅ NLog.config                          # 日志配置
✅ src/
   ✅ CadAutomationPlugin/              # 主插件
      ✅ PluginEntryPoint.cs
      ✅ Commands/
         ✅ SmartDimensionCommands.cs
         ✅ BOMCommands.cs
         ✅ BatchChangeCommands.cs
         ✅ UnfoldCommands.cs
         ✅ ParametricCommands.cs
   ✅ Core/                             # 核心业务
      ✅ SmartDimensionEngine.cs
      ✅ BOMGenerator.cs
      ✅ ChangePropagationEngine.cs
      ✅ UnfoldEngine.cs
      ✅ ParametricDrawingGenerator.cs
   ✅ UI/                               # WPF 界面
      ✅ Views/MainWindow.xaml
      ✅ ViewModels/MainViewModel.cs
   ✅ Data/                             # 数据处理
      ✅ Excel/ExcelExporter.cs
   ✅ Shared/                           # 共享工具
      ✅ Logging/LogManager.cs
      ✅ Geometry/GeometryUtils.cs
✅ tests/                               # 单元测试
```

---

## ✅ 验证清单

编译完成后，确认以下项目：

- [ ] `dotnet --version` 显示 6.0.x
- [ ] `dotnet restore` 成功
- [ ] `dotnet build` 显示 0 错误
- [ ] DLL 文件生成在 `bin\Release\net6.0-windows\`
- [ ] AutoCAD 成功加载插件
- [ ] `SMARTDIM` 命令可用
- [ ] `GENBOM` 命令可用
- [ ] 主窗口可以打开（输入 `CADAUTO`）

---

## 📞 需要帮助？

如果遇到问题：

1. 查看 `BUILD_REPORT.md` 获取详细测试报告
2. 检查 AutoCAD 命令行错误信息
3. 查看日志文件：`%APPDATA%\CadAutomationPlugin\logs\`

---

**文档版本**: 1.0  
**更新日期**: 2026-03-11  
**适用系统**: Windows 10/11 + AutoCAD 2020+
