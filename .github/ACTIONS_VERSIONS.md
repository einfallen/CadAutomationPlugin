# GitHub Actions 版本说明

本文档记录项目使用的 GitHub Actions 版本信息。

---

## 📦 当前使用的 Actions 版本

| Action | 版本 | 说明 |
|--------|------|------|
| `actions/checkout` | v4 | 代码检出 |
| `actions/setup-dotnet` | v4 | .NET SDK 安装 |
| `actions/upload-artifact` | v4 | 构建产物上传 |
| `actions/download-artifact` | v4 | 构建产物下载 |
| `actions/cache` | v4 | 缓存管理 |
| `github/codeql-action` | v3 | 代码分析上传 |
| `softprops/action-gh-release` | v1 | GitHub Release 创建 |

---

## 🔄 版本历史

### 2026-03-12 - 升级到 v4

**原因**: GitHub 官方弃用通知

- `actions/upload-artifact@v3` 于 2024-04-16 弃用
- `actions/checkout@v3` 和 `actions/setup-dotnet@v3` 建议使用最新版本

**变更**:
```diff
- uses: actions/checkout@v3
+ uses: actions/checkout@v4

- uses: actions/setup-dotnet@v3
+ uses: actions/setup-dotnet@v4

- uses: actions/upload-artifact@v3
+ uses: actions/upload-artifact@v4

- uses: github/codeql-action/upload-sarif@v2
+ uses: github/codeql-action/upload-sarif@v3
```

---

## ⚠️ 弃用通知

### actions/upload-artifact v3

**弃用日期**: 2024-04-16  
**移除日期**: 2024-12-03（预计）

**影响**: 使用 v3 的工作流会自动失败

**解决方案**: 升级到 v4

参考：https://github.blog/changelog/2024-04-16-deprecation-notice-v3-of-the-artifact-actions/

---

## 🔍 检查 Actions 版本

### 检查本地工作流

```bash
# 查找所有使用的 Actions
grep -r "uses: actions/" .github/workflows/
```

### 检查最新版本

访问 GitHub Marketplace:
- https://github.com/marketplace/actions/checkout
- https://github.com/marketplace/actions/setup-dotnet
- https://github.com/marketplace/actions/upload-a-build-artifact

### 自动检查工具

使用 [Dependabot](https://docs.github.com/en/code-security/dependabot/working-with-dependabot/keeping-your-actions-up-to-date) 自动更新 Actions:

```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
```

---

## 📋 最佳实践

### 1. 使用主要版本号

```yaml
# ✅ 推荐：锁定主要版本
uses: actions/checkout@v4

# ❌ 不推荐：使用 latest 或分支
uses: actions/checkout@main
uses: actions/checkout@latest
```

### 2. 定期更新

- 订阅 GitHub Changelog
- 使用 Dependabot 自动检查
- 每季度审查一次工作流

### 3. 测试后再部署

更新 Actions 版本后：
1. 在测试分支验证
2. 手动触发工作流测试
3. 确认无误后合并到 main

---

## 🐛 常见问题

### Q: 更新后工作流失败？

**A**: 检查 breaking changes

每个主要版本更新可能有行为变化，查看：
- Release Notes
- Migration Guide
- GitHub Changelog

### Q: 如何回滚？

**A**: 修改工作流文件，恢复旧版本

```yaml
# 从 v4 回滚到 v3（不推荐，仅临时）
uses: actions/checkout@v3
```

然后重新推送。

---

## 📞 相关资源

- [GitHub Actions 文档](https://docs.github.com/en/actions)
- [GitHub Changelog](https://github.blog/changelog/)
- [Actions Marketplace](https://github.com/marketplace?type=actions)
- [Dependabot 配置](https://docs.github.com/en/code-security/dependabot/dependabot-version-updates/configuring-dependabot-version-updates)

---

**最后更新**: 2026-03-12  
**当前状态**: ✅ 所有 Actions 使用最新版本
