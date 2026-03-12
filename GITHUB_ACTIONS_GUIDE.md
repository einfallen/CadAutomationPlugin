# 🚀 GitHub Actions 推送指南

## ✅ 当前状态

- ✅ Git 仓库已初始化
- ✅ 首次提交完成（35 个文件）
- ✅ GitHub Actions 配置已就绪
- ✅ 分支已重命名为 `main`

---

## 📋 推送步骤

### 步骤 1: 在 GitHub 创建仓库

1. 访问：**https://github.com/new**
2. 填写以下信息：

```
Repository name: CadAutomationPlugin
Description: AutoCAD 自动化插件 - 智能标注/BOM 生成/批量改图
Visibility: ⚪ Private (推荐) 或 ⚫ Public
```

3. **不要勾选**以下选项：
   - ❌ Add a README file
   - ❌ Add .gitignore
   - ❌ Add a license

4. 点击 **"Create repository"**

---

### 步骤 2: 复制仓库地址

创建成功后，页面显示类似：

```
https://github.com/YOUR_USERNAME/CadAutomationPlugin.git
```

**复制这个地址！**

---

### 步骤 3: 添加远程仓库并推送

在服务器执行以下命令：

```bash
# 进入项目目录
cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 添加远程仓库（替换为你的地址）
git remote add origin https://github.com/YOUR_USERNAME/CadAutomationPlugin.git

# 推送到 GitHub
git push -u origin main
```

**如果提示输入密码：**
- 使用 GitHub Personal Access Token
- 生成地址：https://github.com/settings/tokens
- 权限：`repo` (Full control of private repositories)

**或使用 SSH（推荐）：**

```bash
# 生成 SSH 密钥（如果没有）
ssh-keygen -t ed25519 -C "your_email@example.com"

# 查看公钥
cat ~/.ssh/id_ed25519.pub

# 复制公钥到 GitHub: https://github.com/settings/keys

# 使用 SSH 地址推送
git remote add origin git@github.com:YOUR_USERNAME/CadAutomationPlugin.git
git push -u origin main
```

---

### 步骤 4: 查看自动构建

推送成功后，GitHub Actions 自动触发：

1. 访问：**https://github.com/YOUR_USERNAME/CadAutomationPlugin/actions**

2. 你会看到构建任务正在运行：

```
✅ Build CAD Plugin #1
   Running · 3m 25s
   Triggered by push to main
```

3. 点击构建记录查看详情

---

### 步骤 5: 等待构建完成

构建过程约 **3-5 分钟**，包含：

```
✅ Setup .NET 6.0          (30 秒)
✅ Restore dependencies     (1 分钟)
✅ Build                    (1 分钟)
✅ Publish                  (1 分钟)
✅ Upload artifacts         (30 秒)
```

---

### 步骤 6: 下载构建产物

构建成功后：

1. 点击构建记录（绿色对勾 ✓）
2. 滚动到页面底部
3. 找到 **"Artifacts"** 区域
4. 点击以下任一文件下载：

```
📦 CadAutomationPlugin          (完整输出目录)
📦 CadAutomationPlugin-Installer (ZIP 安装包)
```

---

## 📦 构建产物说明

下载的文件包含：

### CadAutomationPlugin（文件夹）
```
✅ CadAutomationPlugin.dll      # 主插件（AutoCAD 加载这个）
✅ Core.dll                     # 核心业务逻辑
✅ UI.dll                       # WPF 界面
✅ Data.dll                     # Excel/数据处理
✅ Shared.dll                   # 共享工具
✅ EPPlus.dll                   # Excel 库
✅ Newtonsoft.Json.dll          # JSON 库
✅ NLog.dll                     # 日志库
✅ ... (其他依赖)
```

### CadAutomationPlugin-Installer.zip
```
✅ 上述所有文件的压缩包
```

---

## 🎯 在 AutoCAD 中测试

### 下载 DLL 后：

1. **解压文件** 到本地文件夹
   ```
   C:\Projects\CadAutomationPlugin\
   ```

2. **启动 AutoCAD** (2020 或更高版本)

3. **输入命令**: `NETLOAD`

4. **浏览到 DLL**:
   ```
   C:\Projects\CadAutomationPlugin\CadAutomationPlugin.dll
   ```

5. **点击"打开"**

6. **验证加载成功**:
   ```
   ✓ CAD 自动化插件已加载 (输入 CADAUTO 启动)
   ```

7. **测试命令**:
   ```
   SMARTDIM      # 智能标注
   GENBOM        # 生成 BOM 表
   BATCHCHANGE   # 批量改图
   ```

---

## 🔄 后续开发流程

### 修改代码后重新构建

```bash
# 1. 修改代码
# 编辑文件...

# 2. 提交变更
git add .
git commit -m "修复智能标注算法"

# 3. 推送到 GitHub
git push origin main

# 4. 自动触发新的构建
# 访问：https://github.com/YOUR_USERNAME/CadAutomationPlugin/actions
```

### 版本发布

```bash
# 1. 打标签
git tag v1.0.0
git push origin v1.0.0

# 2. GitHub Actions 自动创建 Release
# 访问：https://github.com/YOUR_USERNAME/CadAutomationPlugin/releases
```

---

## 📊 GitHub Actions 配置说明

当前配置位于：`.github/workflows/build.yml`

### 触发条件
```yaml
on:
  push:
    branches: [ main, develop ]    # 推送到这些分支时触发
  pull_request:
    branches: [ main ]             # PR 时触发
  workflow_dispatch:               # 允许手动触发
```

### 构建环境
```yaml
runs-on: windows-latest            # 使用 Windows 最新镜像
```

### 构建步骤
```yaml
1. 检出代码
2. 安装 .NET 6.0
3. 还原 NuGet 包
4. 编译 Release 版本
5. 发布到 ./publish 目录
6. 上传构建产物（保存 30 天）
7. 创建 ZIP 安装包
```

---

## 💰 免费额度说明

### GitHub Actions 免费计划

| 项目 | 额度 |
|------|------|
| 构建分钟数 | 2000 分钟/月 |
| 存储空间 | 500 MB |
| 带宽 | 免费 |

### CAD 插件构建估算

- 单次构建时间：3-5 分钟
- 每天构建 2 次 × 5 分钟 × 30 天 = **300 分钟/月**
- ✅ **远低于免费额度**

---

## 🐛 常见问题

### Q1: 推送时提示认证失败

**解决**:
```bash
# 使用 Personal Access Token
# 1. 访问：https://github.com/settings/tokens
# 2. 生成新 Token (权限：repo)
# 3. 使用 Token 代替密码

# 或使用 SSH
git remote set-url origin git@github.com:YOUR_USERNAME/CadAutomationPlugin.git
```

### Q2: Actions 没有触发

**检查**:
1. 确认 `.github/workflows/build.yml` 存在
2. 检查分支名称是否为 `main`
3. 查看 Settings → Actions 是否启用

### Q3: 构建失败

**排查步骤**:
1. 点击构建记录查看日志
2. 检查错误信息
3. 常见问题：
   - .NET SDK 版本不匹配
   - AutoCAD 引用路径错误
   - 依赖包缺失

### Q4: 找不到 Artifacts

**解决**:
- 确保构建成功（绿色对勾）
- 滚动到页面最底部
- Artifacts 在日志区域下方

---

## ✅ 验证清单

推送前确认：

- [ ] GitHub 账号已注册
- [ ] 仓库已创建（未初始化）
- [ ] 已复制仓库地址
- [ ] 本地代码已提交
- [ ] `.github/workflows/build.yml` 存在

推送后确认：

- [ ] `git push` 成功
- [ ] Actions 页面显示构建中
- [ ] 构建成功完成（绿色对勾）
- [ ] Artifacts 可下载
- [ ] DLL 可在 AutoCAD 中加载

---

## 📞 需要帮助？

### 文档
- `CLOUD_BUILD.md` - 云构建详细指南
- `CONFIG_COMPLETE.md` - 配置完成报告
- `WINDOWS_BUILD.md` - Windows 测试指南

### GitHub 资源
- Actions 文档：https://docs.github.com/en/actions
- 价格说明：https://github.com/pricing
- SSH 连接：https://docs.github.com/en/authentication

---

## 🎉 准备就绪！

**执行以下命令开始推送：**

```bash
cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 替换为你的仓库地址
git remote add origin https://github.com/YOUR_USERNAME/CadAutomationPlugin.git

# 推送（触发自动构建）
git push -u origin main
```

**3-5 分钟后即可下载编译好的 DLL！**

---

**更新时间**: 2026-03-11 18:27  
**状态**: 等待推送到 GitHub
