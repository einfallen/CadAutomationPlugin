#if !CLOUD_BUILD
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Shared.Logging;

namespace CadAutomationPlugin.Core.BOM
{
    /// <summary>
    /// BOM 表生成引擎 - 核心功能 2
    /// 提取装配体结构，生成包含重量的 BOM 表
    /// </summary>
    public class BOMGenerator
    {
        private static readonly ILogger Logger = LogManager.GetLogger(nameof(BOMGenerator));

        /// <summary>
        /// 提取 BOM 数据
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="trans">当前事务</param>
        /// <returns>BOM 数据</returns>
        /// <exception cref="ArgumentNullException">当 db 或 trans 为 null 时</exception>
        public BOMData ExtractBOM(Database db, Transaction trans)
        {
            if (db == null)
            {
                Logger.Error("ExtractBOM: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (trans == null)
            {
                Logger.Error("ExtractBOM: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            Logger.Info("开始提取 BOM 数据");

            var bomData = new BOMData();

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null)
            {
                Logger.Error("ExtractBOM: 无法获取块表");
                return bomData;
            }

            // 遍历所有块定义
            foreach (ObjectId blockId in blockTable)
            {
                try
                {
                    using var block = trans.GetObject(blockId, OpenMode.ForRead) as BlockTableRecord;
                    if (block == null || block.IsLayout) continue;

                    // 跳过非装配块
                    if (!IsAssemblyBlock(block)) continue;

                    var item = ExtractBlockInfo(block, trans);
                    if (item != null)
                    {
                        bomData.Items.Add(item);
                        Logger.Debug($"提取零件：{item.PartNumber} - {item.Name}");
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"处理块时发生错误：{blockId.ObjectClass.DxfName}", ex);
                }
            }

            // 计算数量
            CalculateQuantities(bomData, db, trans);

            Logger.Info($"BOM 提取完成，共 {bomData.Items.Count} 个零件");
            return bomData;
        }

        /// <summary>
        /// 提取带重量的 BOM 数据
        /// </summary>
        /// <param name="db">AutoCAD 数据库</param>
        /// <param name="trans">当前事务</param>
        /// <returns>带重量的 BOM 数据</returns>
        public BOMData ExtractBOMWithWeight(Database db, Transaction trans)
        {
            if (db == null)
            {
                Logger.Error("ExtractBOMWithWeight: db 为 null");
                throw new ArgumentNullException(nameof(db));
            }

            if (trans == null)
            {
                Logger.Error("ExtractBOMWithWeight: trans 为 null");
                throw new ArgumentNullException(nameof(trans));
            }

            Logger.Info("开始提取 BOM 数据（含重量）");

            var bomData = ExtractBOM(db, trans);

            foreach (var item in bomData.Items)
            {
                try
                {
                    item.UnitWeight = CalculateWeight(item, db, trans);
                    item.TotalWeight = item.UnitWeight * item.Quantity;
                    Logger.Debug($"计算重量：{item.PartNumber} = {item.UnitWeight:F3} kg");
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"计算重量失败：{item.PartNumber}", ex);
                    item.UnitWeight = 0;
                    item.TotalWeight = 0;
                }
            }

            return bomData;
        }

        /// <summary>
        /// 提取块信息
        /// </summary>
        private BOMItem? ExtractBlockInfo(BlockTableRecord block, Transaction trans)
        {
            if (block == null || block.IsDisposed)
            {
                Logger.Warn("ExtractBlockInfo: 块对象无效");
                return null;
            }

            try
            {
                var item = new BOMItem
                {
                    PartNumber = block.Name,
                    Name = GetBlockAttribute(block, "NAME", trans) ?? block.Name,
                    Material = GetBlockAttribute(block, "MATERIAL", trans) ?? "未知",
                    Description = GetBlockAttribute(block, "DESC", trans) ?? "",
                    Vendor = GetBlockAttribute(block, "VENDOR", trans) ?? ""
                };

                return item;
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"提取块信息失败：{block.Name}", ex);
                return null;
            }
            catch (System.Exception ex)
            {
                Logger.Error($"提取块信息失败（未知错误）：{block.Name}", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取块属性
        /// </summary>
        private string? GetBlockAttribute(BlockTableRecord block, string tagName, Transaction trans)
        {
            if (block == null || block.IsDisposed)
            {
                return null;
            }

            try
            {
                foreach (ObjectId id in block)
                {
                    using var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity is DBText text)
                    {
                        if (text.Text.ToUpper().Contains(tagName))
                        {
                            return text.Text;
                        }
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"读取块属性失败：{tagName}", ex);
            }

            return null;
        }

        /// <summary>
        /// 判断是否为装配块
        /// </summary>
        private bool IsAssemblyBlock(BlockTableRecord block)
        {
            if (block == null || block.IsDisposed)
            {
                return false;
            }

            // 排除系统块和布局
            if (block.Name.StartsWith("*") || block.IsLayout)
                return false;

            // 检查是否包含几何实体
            foreach (ObjectId id in block)
            {
                return true; // 有内容即视为装配块
            }

            return false;
        }

        /// <summary>
        /// 计算零件数量
        /// </summary>
        private void CalculateQuantities(BOMData bomData, Database db, Transaction trans)
        {
            if (bomData == null || db == null || trans == null)
            {
                Logger.Warn("CalculateQuantities: 参数无效");
                return;
            }

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null)
            {
                Logger.Error("CalculateQuantities: 无法获取块表");
                return;
            }

            using var modelSpace = trans.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForRead) as BlockTableRecord;
            if (modelSpace == null)
            {
                Logger.Error("CalculateQuantities: 无法获取模型空间");
                return;
            }

            // 统计模型空间中各块的引用次数
            var quantityDict = new Dictionary<string, int>();

            foreach (ObjectId id in modelSpace)
            {
                try
                {
                    using var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity is BlockReference blockRef)
                    {
                        var blockName = blockRef.BlockTableRecord.Name;
                        if (quantityDict.ContainsKey(blockName))
                            quantityDict[blockName]++;
                        else
                            quantityDict[blockName] = 1;
                    }
                }
                catch (Autodesk.AutoCAD.Runtime.Exception ex)
                {
                    Logger.Error($"统计块引用失败", ex);
                }
            }

            // 更新 BOM 项数量
            foreach (var item in bomData.Items)
            {
                if (quantityDict.TryGetValue(item.PartNumber, out int qty))
                {
                    item.Quantity = qty;
                }
            }
        }

        /// <summary>
        /// 计算零件重量
        /// </summary>
        private double CalculateWeight(BOMItem item, Database db, Transaction trans)
        {
            if (item == null || db == null || trans == null)
            {
                Logger.Warn("CalculateWeight: 参数无效");
                return 0;
            }

            try
            {
                // 获取零件体积（需要 3D 实体）
                var volume = GetPartVolume(item.PartNumber, db, trans);
                
                // 获取材料密度
                var density = GetMaterialDensity(item.Material);

                // 重量 = 体积 × 密度
                var weight = volume * density / 1000000; // 转换为 kg

                return Math.Round(weight, 3);
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"重量计算失败：{item.PartNumber}", ex);
                return 0;
            }
            catch (System.Exception ex)
            {
                Logger.Error($"重量计算失败（未知错误）：{item.PartNumber}", ex);
                return 0;
            }
        }

        /// <summary>
        /// 获取零件体积
        /// </summary>
        private double GetPartVolume(string partNumber, Database db, Transaction trans)
        {
            if (string.IsNullOrEmpty(partNumber) || db == null || trans == null)
            {
                return 0;
            }

            using var blockTable = trans.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            if (blockTable == null || !blockTable.Has(partNumber))
            {
                return 0;
            }
            
            using var block = trans.GetObject(blockTable[partNumber], OpenMode.ForRead) as BlockTableRecord;
            if (block == null || block.IsDisposed)
            {
                return 0;
            }

            double totalVolume = 0;

            try
            {
                foreach (ObjectId id in block)
                {
                    using var entity = trans.GetObject(id, OpenMode.ForRead) as Entity;
                    if (entity is Solid3d solid)
                    {
                        totalVolume += solid.Volume;
                    }
                }
            }
            catch (Autodesk.AutoCAD.Runtime.Exception ex)
            {
                Logger.Error($"计算体积失败：{partNumber}", ex);
            }

            return totalVolume;
        }

        /// <summary>
        /// 获取材料密度 (g/cm³)
        /// </summary>
        private double GetMaterialDensity(string material)
        {
            if (string.IsNullOrEmpty(material))
            {
                return 7.85; // 默认钢密度
            }

            var densityTable = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "Q235", 7.85 },
                { "45#", 7.85 },
                { "304", 7.93 },
                { "316L", 7.98 },
                { "AL6061", 2.70 },
                { "AL5052", 2.68 },
                { "CU", 8.96 },
                { "Brass", 8.50 },
                { "尼龙", 1.15 },
                { "POM", 1.42 },
                { "PTFE", 2.20 }
            };

            return densityTable.TryGetValue(material, out var density) ? density : 7.85; // 默认钢密度
        }
    }

    /// <summary>
    /// BOM 数据结构
    /// </summary>
    public class BOMData
    {
        public List<BOMItem> Items { get; set; } = new List<BOMItem>();
        public string AssemblyName { get; set; } = "";
        public DateTime GeneratedDate { get; set; } = DateTime.Now;
        public string GeneratedBy { get; set; } = Environment.UserName;
    }

    /// <summary>
    /// BOM 表项
    /// </summary>
    public class BOMItem
    {
        public int Index { get; set; }
        public string PartNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public string Material { get; set; } = "";
        public double UnitWeight { get; set; }
        public double TotalWeight { get; set; }
        public string Vendor { get; set; } = "";
        public string Description { get; set; } = "";
        public string Remarks { get; set; } = "";
    }
}
#else
// 云编译存根
namespace CadAutomationPlugin.Core.BOM
{
    public class BOMGeneratorStub { }
    
    public class BOMData
    {
        public List<BOMItem> Items { get; set; } = new List<BOMItem>();
        public string ProjectName { get; set; } = "";
        public string DrawingNumber { get; set; } = "";
        public System.DateTime GeneratedAt { get; set; } = System.DateTime.Now;
        public string GeneratedBy { get; set; } = "";
    }

    public class BOMItem
    {
        public int Index { get; set; }
        public string PartNumber { get; set; } = "";
        public string Name { get; set; } = "";
        public int Quantity { get; set; } = 1;
        public string Material { get; set; } = "";
        public double UnitWeight { get; set; }
        public double TotalWeight { get; set; }
        public string Vendor { get; set; } = "";
        public string Description { get; set; } = "";
        public string Remarks { get; set; } = "";
    }
}
#endif
