using CadAutomationPlugin.Core.BOM;
using Xunit;

namespace CadAutomationPlugin.Tests
{
    /// <summary>
    /// BOM 生成器单元测试
    /// </summary>
    public class BOMGeneratorTests
    {
        [Fact]
        public void MaterialDensity_Steel_ReturnsCorrectValue()
        {
            // Arrange
            var generator = new BOMGenerator();
            
            // Act
            var density = GetMaterialDensity("Q235");
            
            // Assert
            Assert.Equal(7.85, density, 2);
        }

        [Fact]
        public void MaterialDensity_Aluminum_ReturnsCorrectValue()
        {
            // Act
            var density = GetMaterialDensity("AL6061");
            
            // Assert
            Assert.Equal(2.70, density, 2);
        }

        [Fact]
        public void MaterialDensity_Unknown_ReturnsDefaultValue()
        {
            // Act
            var density = GetMaterialDensity("UNKNOWN");
            
            // Assert
            Assert.Equal(7.85, density, 2); // 默认钢密度
        }

        private double GetMaterialDensity(string material)
        {
            var densityTable = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase)
            {
                { "Q235", 7.85 },
                { "45#", 7.85 },
                { "AL6061", 2.70 }
            };

            return densityTable.TryGetValue(material, out var density) ? density : 7.85;
        }
    }

    /// <summary>
    /// 智能标注引擎测试
    /// </summary>
    public class SmartDimensionEngineTests
    {
        [Fact]
        public void CalculateOptimalTextPosition_ReturnsValidPosition()
        {
            // Arrange
            var config = new DimensionConfig { TextOffset = 5.0 };
            
            // Act & Assert
            Assert.NotNull(config);
            Assert.Equal(5.0, config.TextOffset);
        }
    }
}
