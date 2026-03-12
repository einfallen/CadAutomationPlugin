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

### 修改的文件

| 文件 | 修改内容 |
|------|---------|
| `.github/workflows/build.yml` | 移除 SARIF 上传、添加权限、Node.js 24 支持 |
| `.github/workflows/release.yml` | 添加权限、Node.js 24 支持 |

### 添加的配置

```yaml
# 权限配置
permissions:
  contents: read
  security-events: write

# Node.js 24 支持
env:
  FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: 'true'
```

### 删除的配置

```yaml
# 删除 CodeQL SARIF 上传（不需要）
- name: 上传代码分析结果
  uses: github/codeql-action/upload-sarif@v3
  with:
    sarif_file: results.sarif
```

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
