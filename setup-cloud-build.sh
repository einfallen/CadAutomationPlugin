# 云构建快速配置脚本

echo "=========================================="
echo "  CAD 插件 - 云构建配置向导"
echo "=========================================="
echo ""

# 检查 Git 是否安装
if ! command -v git &> /dev/null; then
    echo "❌ Git 未安装，请先安装 Git"
    exit 1
fi

echo "✅ Git 已安装"
echo ""

# 初始化 Git 仓库
echo "📦 初始化 Git 仓库..."
cd /home/admin/openclaw/workspace/CadAutomationPlugin

if [ ! -d ".git" ]; then
    git init
    echo "✅ Git 仓库已初始化"
else
    echo "⚠️ Git 仓库已存在"
fi

echo ""

# 添加所有文件
echo "📝 添加文件到 Git..."
git add .
echo "✅ 文件已添加"

echo ""

# 配置用户信息（如果未配置）
if [ -z "$(git config --global user.name)" ]; then
    echo "⚙️  配置 Git 用户信息..."
    read -p "请输入 Git 用户名：" git_user
    git config --global user.name "$git_user"
    
    read -p "请输入 Git 邮箱：" git_email
    git config --global user.email "$git_email"
fi

echo ""

# 首次提交
echo "💾 创建首次提交..."
git commit -m "Initial commit: CAD Automation Plugin v1.0

功能列表:
- 智能标注 (SMARTDIM)
- BOM 表生成 (GENBOM)
- 批量改图 (BATCHCHANGE)
- 展开图生成 (GENUNFOLD)
- 参数化出图 (GENDRAWING)
- 工艺文件生成 (GENPROCESS)

技术栈:
- AutoCAD .NET API
- .NET 6.0
- WPF UI
- EPPlus (Excel)
"

echo "✅ 首次提交完成"

echo ""
echo "=========================================="
echo "  下一步操作"
echo "=========================================="
echo ""
echo "1️⃣  在 GitHub 创建新仓库:"
echo "   https://github.com/new"
echo ""
echo "2️⃣  复制仓库地址，然后运行:"
echo "   git remote add origin https://github.com/YOUR_USERNAME/CadAutomationPlugin.git"
echo ""
echo "3️⃣  推送到 GitHub（自动触发云构建）:"
echo "   git push -u origin main"
echo ""
echo "4️⃣  查看构建状态:"
echo "   https://github.com/YOUR_USERNAME/CadAutomationPlugin/actions"
echo ""
echo "=========================================="
echo ""

# 显示当前状态
echo "📊 当前 Git 状态:"
git status

echo ""
echo "📁 项目文件:"
ls -la

echo ""
echo "✅ 配置完成！"
