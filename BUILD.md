# 构建说明

## 前置条件

1. 安装 Visual Studio 2022
2. 安装 .NET 6.0 SDK 或 .NET 8.0 SDK
3. 安装 AutoCAD 2020+
4. 设置环境变量 `AutoCADPath` 指向 AutoCAD 安装目录

## 环境变量配置

在系统环境变量中添加：

```
AutoCADPath=C:\Program Files\Autodesk\AutoCAD 2024
AutoCADPluginPath=C:\ProgramData\Autodesk\ApplicationPlugins\CadAutomationPlugin.bundle\Contents
```

## 使用 Visual Studio 构建

1. 打开 `CadAutomationPlugin.sln`
2. 选择 `Release` 配置
3. 按 `Ctrl+Shift+B` 或选择 生成 > 生成解决方案
4. 输出在 `src\CadAutomationPlugin\bin\Release\net6.0-windows\`

## 使用命令行构建

```bash
# 还原依赖
dotnet restore

# 调试构建
dotnet build

# 发布构建
dotnet publish --configuration Release --output ./publish
```

## 调试

1. 在 VS 中右键 `CadAutomationPlugin` 项目 > 属性
2. 调试 > 启动外部程序：`$(AutoCADPath)\acad.exe`
3. 按 F5 启动调试

## 打包发布

```bash
# 使用 WiX 工具集创建 MSI 安装包
# 或使用 Inno Setup
```

## 测试

```bash
# 运行单元测试
dotnet test
```

## 代码风格

遵循 .NET 编码规范：
- 使用 `async/await` 处理异步操作
- 所有公共 API 添加 XML 文档注释
- 使用 `ILogger` 进行日志记录
- 异常处理使用 try-catch，记录日志

## 依赖项

- AutoCAD .NET API (AcMgd, AcDbMgd, AcCoreMgd)
- EPPlus (Excel 处理)
- Newtonsoft.Json (JSON 序列化)
- NLog (日志)
- CommunityToolkit.Mvvm (MVVM 框架)
- Microsoft.Data.Sqlite (SQLite 数据库)
- Dapper (ORM)
