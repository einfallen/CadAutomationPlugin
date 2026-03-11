# 云构建配置说明

## 🌐 支持的云构建平台

| 平台 | 免费额度 | Windows 支持 | 推荐度 |
|------|---------|-------------|--------|
| **GitHub Actions** | 2000 分钟/月 | ✅ windows-latest | ⭐⭐⭐⭐⭐ |
| **Azure DevOps** | 1800 分钟/月 | ✅ Windows 10 | ⭐⭐⭐⭐⭐ |
| **AppVeyor** | 免费 | ✅ Windows | ⭐⭐⭐⭐ |
| **GitLab CI** | 400 分钟/月 | ✅ Windows | ⭐⭐⭐⭐ |

---

## 方案一：GitHub Actions（推荐）

### 步骤 1: 创建 GitHub 仓库

```bash
# 在 GitHub 创建新仓库
# 例如：https://github.com/yourname/CadAutomationPlugin
```

### 步骤 2: 推送代码到 GitHub

```bash
cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 初始化 Git
git init

# 添加远程仓库
git remote add origin https://github.com/YOUR_USERNAME/CadAutomationPlugin.git

# 添加所有文件
git add .

# 提交
git commit -m "Initial commit: CAD Automation Plugin"

# 推送
git push -u origin main
```

### 步骤 3: 自动构建

**推送后自动触发**：
- 每次 `git push` 都会触发云端编译
- 编译产物保存在 Actions 页面
- 可下载 DLL 或 ZIP 包

### 步骤 4: 查看构建结果

1. 访问：`https://github.com/YOUR_USERNAME/CadAutomationPlugin/actions`
2. 点击最新的构建记录
3. 在 "Artifacts" 区域下载构建产物

---

## 方案二：Azure DevOps

### 步骤 1: 创建 Azure DevOps 项目

1. 访问：https://dev.azure.com/
2. 创建新组织（如果没有）
3. 创建新项目：`CadAutomationPlugin`

### 步骤 2: 创建构建管道

1. 进入项目 → Pipelines → Create Pipeline
2. 选择代码源（GitHub 或 Azure Repos）
3. 选择 "Existing Azure Pipelines YAML file"
4. 使用下方配置文件

### 步骤 3: 运行构建

点击 "Run" 触发构建

---

## 方案三：AppVeyor

### 步骤 1: 注册 AppVeyor

1. 访问：https://ci.appveyor.com/
2. 使用 GitHub 账号登录
3. 添加新项目

### 步骤 2: 添加 appveyor.yml

在项目根目录创建 `appveyor.yml`（见下方）

### 步骤 3: 触发构建

推送代码到 GitHub 自动触发

---

## 📋 配置文件

### GitHub Actions: `.github/workflows/build.yml`

```yaml
name: Build CAD Plugin

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:  # 手动触发

jobs:
  build-windows:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET 6.0
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '6.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Publish
      run: dotnet publish --configuration Release --no-build --output ./publish
    
    - name: Upload artifacts
      uses: actions/upload-artifact@v3
      with:
        name: CadAutomationPlugin
        path: ./publish/
        retention-days: 30
```

### Azure DevOps: `azure-pipelines.yml`

```yaml
trigger:
  - main

pool:
  vmImage: 'windows-latest'

steps:
- task: UseDotNet@2
  inputs:
    packageType: 'sdk'
    version: '6.x'

- script: dotnet restore
  displayName: 'Restore dependencies'

- script: dotnet build --configuration Release --no-restore
  displayName: 'Build'

- script: dotnet publish --configuration Release --no-build --output $(Build.ArtifactStagingDirectory)
  displayName: 'Publish'

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: '$(Build.ArtifactStagingDirectory)'
    ArtifactName: 'CadAutomationPlugin'
    publishLocation: 'Container'
```

### AppVeyor: `appveyor.yml`

```yaml
version: 1.0.{build}

image: Visual Studio 2022

install:
  - cmd: dotnet --version

build_script:
  - cmd: dotnet restore
  - cmd: dotnet build --configuration Release
  - cmd: dotnet publish --configuration Release --output ./publish

artifacts:
  - path: publish\
    name: CadAutomationPlugin
```

---

## 🚀 快速开始（GitHub Actions）

### 1. 创建 GitHub 账号

访问：https://github.com/signup

### 2. 创建新仓库

```
Repository name: CadAutomationPlugin
Visibility: Private (或 Public)
```

### 3. 推送代码

```bash
cd /home/admin/openclaw/workspace/CadAutomationPlugin

git init
git remote add origin https://github.com/YOUR_USERNAME/CadAutomationPlugin.git
git add .
git commit -m "Initial commit"
git push -u origin main
```

### 4. 等待自动构建

推送后约 2-5 分钟，构建自动完成

### 5. 下载构建产物

```
Actions → 最新构建 → Artifacts → 下载
```

---

## 📊 构建产物

编译完成后，你将获得：

| 文件 | 说明 | 用途 |
|------|------|------|
| `CadAutomationPlugin.dll` | 主插件 DLL | AutoCAD 加载 |
| `Core.dll` | 核心业务逻辑 | 依赖库 |
| `UI.dll` | WPF 界面 | 用户界面 |
| `Data.dll` | 数据处理 | Excel/数据库 |
| `Shared.dll` | 共享工具 | 日志/几何 |
| `CadAutomationPlugin.zip` | 完整包 | 分发安装 |

---

## 🔧 高级配置

### 自动创建 Release

在 `.github/workflows/build.yml` 添加：

```yaml
- name: Create Release
  uses: softprops/action-gh-release@v1
  if: startsWith(github.ref, 'refs/tags/')
  with:
    files: CadAutomationPlugin.zip
```

### 自动测试

```yaml
- name: Run tests
  run: dotnet test --no-restore --verbosity normal
```

### 代码质量检查

```yaml
- name: Code analysis
  run: dotnet build /p:RunAnalyzersDuringBuild=true
```

---

## 💰 费用说明

| 平台 | 免费额度 | 超出费用 |
|------|---------|---------|
| GitHub Actions | 2000 分钟/月 | $0.008/分钟 |
| Azure DevOps | 1800 分钟/月 | $0.004/分钟 |
| AppVeyor | 免费 | $59/月起 |

**CAD 插件编译时间**: 约 3-5 分钟/次

**月度使用估算**:
- 每天编译 2 次 × 5 分钟 × 30 天 = 300 分钟/月
- ✅ 在免费额度内

---

## ✅ 验证清单

配置完成后确认：

- [ ] GitHub 仓库已创建
- [ ] `.github/workflows/build.yml` 已添加
- [ ] 代码已推送到 GitHub
- [ ] Actions 页面显示构建中
- [ ] 构建成功完成
- [ ] 可下载构建产物（DLL/ZIP）
- [ ] 在 AutoCAD 中测试 DLL

---

## 📞 需要帮助？

- GitHub Actions 文档：https://docs.github.com/en/actions
- Azure DevOps 文档：https://docs.microsoft.com/azure/devops/pipelines
- .NET 文档：https://docs.microsoft.com/dotnet

---

**配置时间**: 2026-03-11  
**适用平台**: GitHub Actions / Azure DevOps / AppVeyor
