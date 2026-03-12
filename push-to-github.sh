#!/bin/bash
# GitHub Actions 快速推送脚本

echo "=========================================="
echo "  GitHub Actions - 快速推送"
echo "=========================================="
echo ""

cd /home/admin/openclaw/workspace/CadAutomationPlugin

# 检查远程仓库
echo "📊 当前 Git 配置:"
git remote -v

echo ""

if git remote | grep -q '^origin$'; then
    echo "✅ 远程仓库已配置"
else
    echo "❌ 未配置远程仓库"
    echo ""
    echo "请输入你的 GitHub 仓库地址:"
    echo "格式：https://github.com/YOUR_USERNAME/CadAutomationPlugin.git"
    echo "或：git@github.com:YOUR_USERNAME/CadAutomationPlugin.git"
    echo ""
    read -p "仓库地址：" repo_url
    
    if [ -n "$repo_url" ]; then
        git remote add origin "$repo_url"
        echo "✅ 远程仓库已添加"
    else
        echo "⚠️  未输入地址，退出"
        exit 1
    fi
fi

echo ""
echo "📦 当前分支:"
git branch

echo ""
echo "📝 待推送的提交:"
git log --oneline -5

echo ""
echo "=========================================="
echo "  推送确认"
echo "=========================================="
echo ""
read -p "是否推送到 GitHub？(y/n): " confirm

if [ "$confirm" = "y" ] || [ "$confirm" = "Y" ]; then
    echo ""
    echo "🚀 开始推送..."
    git push -u origin main
    
    if [ $? -eq 0 ]; then
        echo ""
        echo "=========================================="
        echo "  ✅ 推送成功！"
        echo "=========================================="
        echo ""
        echo "📊 查看构建状态:"
        echo "   https://github.com/YOUR_USERNAME/CadAutomationPlugin/actions"
        echo ""
        echo "⏱️  构建时间：约 3-5 分钟"
        echo ""
        echo "📦 下载构建产物:"
        echo "   1. 访问 Actions 页面"
        echo "   2. 点击最新构建记录"
        echo "   3. 滚动到页面底部"
        echo "   4. 点击 'CadAutomationPlugin' 下载"
        echo ""
        echo "=========================================="
    else
        echo ""
        echo "❌ 推送失败"
        echo ""
        echo "可能的原因:"
        echo "1. 仓库地址不正确"
        echo "2. 认证失败（需要使用 Token 或 SSH）"
        echo "3. 仓库不存在"
        echo ""
        echo "解决方法:"
        echo "1. 检查仓库地址：git remote -v"
        echo "2. 使用 Personal Access Token:"
        echo "   https://github.com/settings/tokens"
        echo "3. 或使用 SSH 连接:"
        echo "   git remote set-url origin git@github.com:YOUR_USERNAME/CadAutomationPlugin.git"
        echo ""
    fi
else
    echo ""
    echo "⚠️  推送已取消"
    echo ""
    echo "手动推送命令:"
    echo "  git push -u origin main"
    echo ""
fi

echo ""
echo "📖 详细指南："
echo "   GITHUB_ACTIONS_GUIDE.md"
echo ""
