# 版本发布指南

本文档说明如何创建和管理 CadAutomationPlugin 的版本发布。

---

## 🚀 自动 Release 流程

### 触发条件

当推送以下格式的标签时，自动创建 GitHub Release：

| 标签格式 | 示例 | 说明 |
|---------|------|------|
| `v*` | `v1.0.0` | 标准版本 |
| `v*-beta` | `v1.0.0-beta` | 测试版本 |
| `v*-alpha` | `v1.1.0-alpha` | 预览版本 |
| `v*-rc*` | `v2.0.0-rc1` | 候选版本 |
| `release/*` | `release/1.0.0` | 发布分支标签 |

---

## 📋 发布步骤

### 1. 准备发布

确认以下内容已完成：

- [ ] 所有功能已测试通过
- [ ] 代码已合并到 main 分支
- [ ] CHANGELOG.md 已更新
- [ ] 版本号已更新（见下方版本规范）

### 2. 创建标签

```bash
# 切换到 main 分支
git checkout main
git pull origin main

# 创建标签（使用语义化版本号）
git tag -a v1.0.0 -m "Release version 1.0.0"

# 或使用测试版本
git tag -a v1.0.0-beta -m "Beta release 1.0.0"
```

### 3. 推送标签

```bash
# 推送单个标签
git push origin v1.0.0

# 或推送所有标签
git push origin --tags
```

### 4. 等待自动发布

GitHub Actions 会自动：

1. ✅ 检出代码
2. ✅ 编译项目
3. ✅ 运行测试
4. ✅ 创建安装包（ZIP）
5. ✅ 创建 GitHub Release
6. ✅ 上传构建产物

**耗时**: 约 5-8 分钟

### 5. 验证发布

访问发布页面确认：
https://github.com/einfallen/CadAutomationPlugin/releases

检查项目：
- [ ] Release 标题正确
- [ ] 发布说明完整
- [ ] 安装包已上传（CadAutomationPlugin.zip）
- [ ] 源代码包已上传（CadAutomationPlugin-Source.zip）

---

## 📐 版本规范

遵循 [语义化版本 2.0.0](https://semver.org/lang/zh-CN/)

### 版本号格式

```
MAJOR.MINOR.PATCH[-PRERELEASE]
   ↑      ↑      ↑        ↑
   |      |      |        └─ 可选：beta, alpha, rc1
   |      |      └─ 向后兼容的问题修正
   |      └─ 向后兼容的功能性新增
   └─ 不兼容的 API 更改
```

### 版本示例

| 版本 | 说明 | 使用场景 |
|------|------|---------|
| `v1.0.0` | 首个稳定版 | 生产环境 |
| `v1.0.1` | 问题修正 | 生产环境 |
| `v1.1.0` | 新增功能 | 生产环境 |
| `v2.0.0` | 重大变更 | 生产环境 |
| `v1.0.0-beta` | 测试版 | 测试环境 |
| `v1.0.0-alpha` | 预览版 | 开发测试 |
| `v1.0.0-rc1` | 候选版 | 发布前测试 |

---

## 📝 发布说明模板

### 标准发布

```markdown
## 🎉 v1.0.0 发布

### ✨ 新增功能
- 功能 1：说明
- 功能 2：说明

### 🐛 问题修复
- 修复问题 1
- 修复问题 2

### 🔧 改进
- 改进 1
- 改进 2

### 📊 统计
- 新增代码：XXX 行
- 修复问题：X 个
- 贡献者：@user1, @user2
```

### 测试版发布

```markdown
## 🧪 v1.0.0-beta 测试版发布

### ⚠️ 注意事项
- 此版本为测试版，可能存在不稳定问题
- 仅用于测试环境，不建议用于生产

### 🎯 测试重点
- 功能 1 的稳定性
- 功能 2 的性能

### 📝 已知问题
- 问题 1：描述
- 问题 2：描述
```

---

## 🔧 手动发布（可选）

如果自动发布失败，可手动创建 Release：

### 1. 下载构建产物

访问 Actions 页面：
https://github.com/einfallen/CadAutomationPlugin/actions

找到对应的构建记录，下载 artifacts。

### 2. 手动创建 Release

1. 访问：https://github.com/einfallen/CadAutomationPlugin/releases/new
2. 输入标签名：`v1.0.0`
3. 发布标题：`Release v1.0.0`
4. 发布说明：粘贴发布说明
5. 上传文件：
   - CadAutomationPlugin.zip
   - CadAutomationPlugin-Source.zip
6. 勾选 "Set as the latest release"
7. 点击 "Publish release"

---

## 📊 发布历史

查看历史发布：
https://github.com/einfallen/CadAutomationPlugin/releases

---

## 🐛 故障排除

### 问题 1: Release 未自动创建

**原因**: 标签格式不匹配

**解决**: 确保标签以 `v` 开头，如 `v1.0.0`

```bash
# 删除错误标签
git tag -d wrong-tag
git push origin :refs/tags/wrong-tag

# 创建正确标签
git tag -a v1.0.0 -m "Release"
git push origin v1.0.0
```

### 问题 2: 构建失败

**原因**: 代码编译错误或测试失败

**解决**:
1. 查看 Actions 日志：https://github.com/einfallen/CadAutomationPlugin/actions
2. 修复错误后重新推送标签

### 问题 3: 安装包过大

**原因**: 包含了不必要的文件

**解决**: 检查 `.gitignore` 和发布配置，排除：
- `bin/`, `obj/` 目录
- `*.pdb` 调试文件
- 大型测试数据

---

## 📚 相关文档

- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [action-gh-release](https://github.com/softprops/action-gh-release)
- [语义化版本规范](https://semver.org/)

---

**最后更新**: 2026-03-12  
**适用版本**: v1.0.0+
