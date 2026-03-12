# 🚀 快速发布命令

## 发布新版本

### 稳定版
```bash
git checkout main
git pull origin main
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

### 测试版
```bash
git tag -a v1.0.0-beta -m "Beta release 1.0.0"
git push origin v1.0.0-beta
```

### 预览版
```bash
git tag -a v1.1.0-alpha -m "Alpha release 1.1.0"
git push origin v1.1.0-alpha
```

### 候选版
```bash
git tag -a v2.0.0-rc1 -m "Release candidate 1"
git push origin v2.0.0-rc1
```

---

## 查看发布

- **发布列表**: https://github.com/einfallen/CadAutomationPlugin/releases
- **构建历史**: https://github.com/einfallen/CadAutomationPlugin/actions

---

## 版本规范

```
v主版本。次版本.修订号 [-测试版]
  ↑      ↑        ↑         ↑
  |      |        |         └─ 可选：beta, alpha, rc1
  |      |        └─ 向后兼容的问题修正
  |      └─ 向后兼容的功能新增
  └─ 不兼容的 API 更改
```

### 示例

| 版本 | 说明 |
|------|------|
| `v1.0.0` | 首个稳定版 |
| `v1.0.1` | 问题修正 |
| `v1.1.0` | 新增功能 |
| `v2.0.0` | 重大变更 |
| `v1.0.0-beta` | 测试版 |
| `v1.0.0-rc1` | 候选版 |

---

## 自动化流程

推送标签后，GitHub Actions 自动执行：

1. ✅ 编译项目（~3 分钟）
2. ✅ 运行测试（~1 分钟）
3. ✅ 创建安装包（~1 分钟）
4. ✅ 创建 GitHub Release（~1 分钟）
5. ✅ 上传构建产物

**总耗时**: 约 5-8 分钟

---

## 验证发布

```bash
# 查看最新标签
git describe --tags --abbrev=0

# 查看所有标签
git tag -l

# 查看远程标签
git ls-remote --tags origin
```

---

## 故障排除

### 删除错误标签
```bash
# 本地删除
git tag -d v1.0.0

# 远程删除
git push origin :refs/tags/v1.0.0
```

### 重新发布
```bash
# 删除旧标签
git tag -d v1.0.0
git push origin :refs/tags/v1.0.0

# 修复问题后重新创建
git tag -a v1.0.0 -m "Release version 1.0.0"
git push origin v1.0.0
```

---

**详细文档**: [RELEASE_GUIDE.md](RELEASE_GUIDE.md)
