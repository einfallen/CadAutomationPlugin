# CAD 自动化插件 - 快速开始指南

## 🎯 项目已完成

✅ **完整代码框架已生成**，包含 7 项核心功能的实现骨架

## 📁 项目位置

```
/home/admin/openclaw/workspace/CadAutomationPlugin/
```

## 📦 项目结构

```
CadAutomationPlugin/
├── 📄 CadAutomationPlugin.sln      # Visual Studio 解决方案
├── 📖 README.md                     # 项目文档
├── 🔧 BUILD.md                      # 构建说明
├── ⚙️  NLog.config                  # 日志配置
├── 📂 src/                          # 源代码
│   ├── CadAutomationPlugin/        # 主插件 (命令入口)
│   ├── Core/                       # 核心业务逻辑
│   ├── UI/                         # WPF 界面
│   ├── Data/                       # 数据处理 (Excel)
│   └── Shared/                     # 共享工具
└── 📂 tests/                       # 单元测试
```

## 🚀 快速启动

### 步骤 1: 安装依赖

```bash
# 安装 .NET 6.0 SDK
# 下载地址：https://dotnet.microsoft.com/download/dotnet/6.0

# 验证安装
dotnet --version
```

### 步骤 2: 配置环境变量

在 Windows 系统环境变量中添加：

```
AutoCADPath=C:\Program Files\Autodesk\AutoCAD 2024
```

### 步骤 3: 编译项目

```bash
cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 还原 NuGet 包
dotnet restore

# 编译
dotnet build --configuration Release
```

### 步骤 4: 加载到 AutoCAD

1. 启动 AutoCAD 2020+
2. 输入命令 `NETLOAD`
3. 选择生成的 DLL: `src/CadAutomationPlugin/bin/Release/net6.0-windows/CadAutomationPlugin.dll`
4. 插件加载成功！

## 📋 可用命令

加载插件后，在 AutoCAD 命令行输入以下命令：

| 命令 | 说明 | 快捷键 |
|------|------|--------|
| `SMARTDIM` | 智能标注 | 自动识别并标注几何特征 |
| `SMARTDIMHOLES` | 标注孔 | 批量标注孔特征 |
| `SMARTDIMCHAMFER` | 标注倒角 | 标注倒角和圆角 |
| `GENBOM` | 生成 BOM | 提取装配体 BOM 表 |
| `GENBOMWEIGHT` | 重量 BOM | BOM 表含重量计算 |
| `BATCHCHANGE` | 批量改图 | 关联零件连锁修改 |
| `SHOWDEPENDENCIES` | 查看关联 | 分析零件依赖关系 |
| `GENUNFOLD` | 展开图 | 钣金件展开为 2D 图 |
| `GENDRAWING` | 参数化出图 | 根据参数生成图纸 |
| `GENPROCESS` | 工艺文件 | 生成工艺文档 |

## 🎨 UI 界面

输入 `CADAUTO` 命令（或加载时自动）打开主窗口：

```
┌─────────────────────────────────────────┐
│  🛠️ CAD 自动化插件 v1.0                  │
├─────────────────────────────────────────┤
│ [📏 智能标注] [📊 BOM 表] [🔄 批量改图]  │
│ [📐 展开图]   [⚙️ 参数化]                │
│                                         │
│  ┌─────────────────────────────────┐   │
│  │ 功能面板...                     │   │
│  └─────────────────────────────────┘   │
│                                         │
│  状态：就绪                    v1.0.0   │
└─────────────────────────────────────────┘
```

## 📝 下一步工作

### 立即可做

1. **编译测试** - 确保代码无编译错误
2. **AutoCAD 加载** - 验证插件能正常加载
3. **命令测试** - 逐个测试命令功能

### 需要完善

1. **智能标注算法** - 完善几何特征识别逻辑
2. **BOM 提取** - 实现完整的装配体遍历
3. **变更传播** - 实现关联关系图算法
4. **展开计算** - 实现钣金 K 因子展开算法

### 可选增强

1. **数据库支持** - 添加 SQLite 存储零件库
2. **模板管理** - 参数化图纸模板系统
3. **批量处理** - 支持文件夹批量处理
4. **云同步** - 配置/模板云同步

## 🐛 故障排查

**问题**: 编译失败，找不到 AutoCAD 引用
- **解决**: 确认 `AutoCADPath` 环境变量正确设置

**问题**: 插件加载后命令不响应
- **解决**: 检查 .NET 版本是否与 AutoCAD 兼容

**问题**: BOM 表重量为 0
- **解决**: 确保零件有 3D 实体几何

## 📚 参考文档

- [README.md](./README.md) - 详细项目说明
- [BUILD.md](./BUILD.md) - 构建指南
- [AutoCAD .NET API 文档](https://help.autodesk.com/view/OARX/2024/ENU/)

---

**生成时间**: 2026-03-11 17:50  
**版本**: v1.0.0 (代码框架)
