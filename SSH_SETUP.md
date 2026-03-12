# 🔐 GitHub SSH 认证配置指南

## ✅ SSH 密钥已生成

**公钥内容**：
```
ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIA/tP38SbsDSYtskoEIGfemVBcf673eKpdO9NgAD4Nyh cad-automation@github
```

---

## 📋 配置步骤

### 步骤 1: 复制公钥

**全选并复制上方公钥内容**（从 `ssh-ed25519` 开始到 `cad-automation@github` 结束）

---

### 步骤 2: 添加到 GitHub

1. 访问：**https://github.com/settings/keys**
2. 点击 **"New SSH key"** 按钮
3. 填写：

```
Title: CadAutomationPlugin Server
Key type: ⚪ Authentication Key
Key: [粘贴上方的公钥内容]
```

4. 点击 **"Add SSH key"**
5. 如果提示输入密码，确认你的 GitHub 密码

---

### 步骤 3: 切换为 SSH 地址

```bash
cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 移除之前的 HTTPS 远程地址
git remote remove origin

# 添加 SSH 远程地址
git remote add origin git@github.com:einfallen/CadAutomationPlugin.git

# 验证配置
git remote -v
```

**预期输出**：
```
origin  git@github.com:einfallen/CadAutomationPlugin.git (fetch)
origin  git@github.com:einfallen/CadAutomationPlugin.git (push)
```

---

### 步骤 4: 测试 SSH 连接

```bash
ssh -T git@github.com
```

**预期输出**：
```
Hi einfallen! You've successfully authenticated, but GitHub does not provide shell access.
```

---

### 步骤 5: 推送代码

```bash
cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 推送（首次需要确认指纹）
git push -u origin main
```

如果提示确认指纹，输入 `yes`

---

## 🚀 推送成功后

### 查看自动构建

访问：**https://github.com/einfallen/CadAutomationPlugin/actions**

你会看到：

```
✅ Build CAD Plugin #1
   Running · 约 3-5 分钟
   Triggered by push to main
```

### 下载构建产物

构建完成后（绿色对勾 ✓）：

1. 点击构建记录
2. 滚动到页面底部 **"Artifacts"** 区域
3. 点击 **`CadAutomationPlugin`** 或 **`CadAutomationPlugin-Installer`** 下载

---

## 📦 快速推送脚本

创建快捷脚本方便后续推送：

```bash
#!/bin/bash
# quick-push.sh - 快速推送到 GitHub

echo "🚀 推送到 GitHub..."

cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 添加所有变更
git add .

# 提交
git commit -m "Auto commit: $(date '+%Y-%m-%d %H:%M')"

# 推送
git push origin main

echo ""
echo "✅ 推送完成！"
echo "📊 查看构建：https://github.com/einfallen/CadAutomationPlugin/actions"
```

使用方法：
```bash
bash quick-push.sh
```

---

## 🔧 常见问题

### Q: 提示 "Permission denied (publickey)"

**解决**：
1. 确认公钥已添加到 GitHub
2. 测试 SSH 连接：`ssh -T git@github.com`
3. 检查 SSH 代理：`ssh-add -l`

### Q: 首次推送需要确认指纹

**解决**：
```bash
# 添加 GitHub 到已知主机
ssh-keyscan github.com >> ~/.ssh/known_hosts
```

### Q: 仓库不存在

**解决**：
1. 确认已在 GitHub 创建仓库
2. 访问：https://github.com/einfallen/CadAutomationPlugin
3. 如果 404，说明仓库未创建，请先创建

---

## ✅ 验证清单

推送前确认：

- [ ] GitHub 账号：einfallen
- [ ] 仓库已创建：https://github.com/einfallen/CadAutomationPlugin
- [ ] SSH 公钥已添加到 GitHub
- [ ] SSH 连接测试成功
- [ ] 远程地址已切换为 SSH

推送后确认：

- [ ] git push 成功
- [ ] Actions 页面显示构建中
- [ ] 3-5 分钟后构建成功
- [ ] 可下载构建产物

---

## 📞 需要帮助？

**下一步操作**：

1. **复制公钥** → 添加到 GitHub
2. **运行命令** 切换 SSH 地址
3. **推送代码** → 触发自动构建

**需要我帮你执行哪一步？**

---

**更新时间**: 2026-03-11 18:35  
**GitHub 账号**: einfallen  
**仓库**: CadAutomationPlugin
