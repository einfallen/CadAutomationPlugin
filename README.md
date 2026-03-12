# CAD 自动化插件

AutoCAD 插件，实现智能标注、BOM 表生成、批量改图等自动化功能。

[![Build CAD Plugin](https://github.com/einfallen/CadAutomationPlugin/actions/workflows/build.yml/badge.svg)](https://github.com/einfallen/CadAutomationPlugin/actions/workflows/build.yml)
[![Create Release](https://github.com/einfallen/CadAutomationPlugin/actions/workflows/release.yml/badge.svg)](https://github.com/einfallen/CadAutomationPlugin/actions/workflows/release.yml)
[![Latest Release](https://img.shields.io/github/v/release/einfallen/CadAutomationPlugin)](https://github.com/einfallen/CadAutomationPlugin/releases/latest)

## 功能列表

### ✅ Phase 1 - 核心功能

| 功能 | 命令 | 说明 |
|------|------|------|
| 📏 智能标注 | `SMARTDIM` | 自动识别几何特征并标注 |
| 📏 标注孔 | `SMARTDIMHOLES` | 批量标注孔特征 |
| 📏 标注倒角 | `SMARTDIMCHAMFER` | 标注倒角和圆角 |
| 📊 生成 BOM | `GENBOM` | 提取装配体 BOM 表 |
| 📊 重量 BOM | `GENBOMWEIGHT` | BOM 表含重量计算 |
| 🔄 批量改图 | `BATCHCHANGE` | 关联零件连锁修改 |
| 🔗 查看关联 | `SHOWDEPENDENCIES` | 分析零件依赖关系 |

### 🚧 Phase 2 - 扩展功能

| 功能 | 命令 | 说明 |
|------|------|------|
| 📐 展开图 | `GENUNFOLD` | 钣金件展开为 2D 图 |
| 📐 批量展开 | `BATCHUNFOLD` | 批量生成展开图 |
| ⚙️ 参数化出图 | `GENDRAWING` | 根据参数生成图纸 |
| ⚙️ 工艺文件 | `GENPROCESS` | 生成工艺文档 |

## 系统要求

- **AutoCAD**: 2020 或更高版本
- **.NET**: .NET 6.0 或 .NET 8.0
- **操作系统**: Windows 10/11 (64 位)
- **Visual Studio**: 2022 (用于开发)

## 安装

### 📥 方法 1: 下载 Release 包（推荐）

1. 访问 [Releases 页面](https://github.com/einfallen/CadAutomationPlugin/releases)
2. 下载最新版本的 `CadAutomationPlugin.zip`
3. 解压到本地
4. 在 AutoCAD 中运行 `NETLOAD` 命令加载 DLL

### 📦 方法 2: MSI 安装包

1. 下载 `CadAutomationPlugin_Setup.msi`
2. 双击运行安装程序
3. 启动 AutoCAD，插件自动加载

### 🔧 方法 3: 手动安装

1. 编译项目生成 `CadAutomationPlugin.dll`
2. 将 DLL 复制到 AutoCAD 支持路径
3. 在 AutoCAD 中运行 `NETLOAD` 命令加载 DLL

### 💻 方法 4: 开发者安装

```bash
# 克隆仓库
git clone https://github.com/einfallen/CadAutomationPlugin.git
cd CadAutomationPlugin

# 还原依赖
dotnet restore

# 编译
dotnet build --configuration Release

# 自动加载到 AutoCAD
# 在 Visual Studio 中按 F5 调试
```

## 使用说明

### 智能标注

1. 在 AutoCAD 命令行输入 `SMARTDIM`
2. 选择要标注的对象
3. 按 Enter 完成标注

### 生成 BOM 表

1. 打开装配体图纸
2. 输入 `GENBOM` 命令
3. 选择保存路径
4. Excel BOM 表自动生成

### 批量改图

1. 输入 `REGISTERLINK` 注册零件关联
2. 修改基准零件
3. 输入 `BATCHCHANGE` 执行连锁修改

## 项目结构

```
CadAutomationPlugin/
├── src/
│   ├── CadAutomationPlugin/    # 主插件项目
│   ├── Core/                   # 核心业务逻辑
│   ├── UI/                     # WPF 用户界面
│   ├── Data/                   # 数据处理 (Excel/DB)
│   └── Shared/                 # 共享工具类
├── tests/                      # 单元测试
├── docs/                       # 文档
└── temp/                       # 临时文件
```

## 开发指南

### 添加新命令

在 `Commands` 文件夹创建新类：

```csharp
public class MyCommands
{
    [CommandMethod("MYCOMMAND")]
    public void MyCommand()
    {
        // 命令实现
    }
}
```

### 调试

1. 在 Visual Studio 中设置 AutoCAD 为启动程序
2. 按 F5 启动调试
3. AutoCAD 自动加载插件

### 构建发布

```bash
dotnet publish --configuration Release --self-contained false
```

### 🚀 自动发布

项目配置了 GitHub Actions 自动发布流程：

```bash
# 1. 创建版本标签
git tag -a v1.0.0 -m "Release version 1.0.0"

# 2. 推送标签（自动触发发布）
git push origin v1.0.0

# 3. 等待 5-8 分钟，Release 自动创建
# 访问：https://github.com/einfallen/CadAutomationPlugin/releases
```

详细说明请参考 [RELEASE_GUIDE.md](RELEASE_GUIDE.md)

## API 参考

### 智能标注引擎

```csharp
var engine = new SmartDimensionEngine();
engine.AutoDimension(database, selection, transaction);
```

### BOM 生成器

```csharp
var generator = new BOMGenerator();
var bomData = generator.ExtractBOMWithWeight(database, transaction);
```

## 常见问题

**Q: 插件加载失败？**
A: 检查 .NET 版本是否匹配，确认 DLL 与 AutoCAD 版本兼容。

**Q: 标注不显示？**
A: 检查标注图层是否可见，标注样式是否正确。

**Q: BOM 表重量为 0？**
A: 确保零件有 3D 实体几何，材料密度已正确设置。

## 许可证

MIT License

## 联系方式

- 邮箱：support@cadautomation.com
- 文档：https://docs.cadautomation.com

---

**版本**: 1.0.0  
**更新日期**: 2026-03-11
