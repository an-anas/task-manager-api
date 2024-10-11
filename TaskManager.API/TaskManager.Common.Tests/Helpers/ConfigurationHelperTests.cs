using Microsoft.Extensions.Configuration;
using Moq;
using TaskManager.Common.Helpers;

namespace TaskManager.Common.Tests.Helpers
{
    [TestFixture]
    public class ConfigurationHelperTests
    {
        private Mock<IConfiguration> _configurationMock;
        private ConfigurationHelper _configHelper;

        [SetUp]
        public void Setup()
        {
            _configurationMock = new Mock<IConfiguration>();
            _configHelper = new ConfigurationHelper(_configurationMock.Object);
        }

        [Test]
        public void GetConfigValue_ReturnsValue_FromEnvironmentVariable()
        {
            // Arrange
            var key = "Some:Key";
            var expectedValue = "EnvValue";
            Environment.SetEnvironmentVariable(key.Replace(":", "__"), expectedValue);

            // Act
            var result = _configHelper.GetConfigValue(key);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetConfigValue_ReturnsValue_FromConfiguration()
        {
            // Arrange
            var key = "Some:Key";
            var expectedValue = "ConfigValue";
            _configurationMock.Setup(c => c[key]).Returns(expectedValue);

            // Act
            var result = _configHelper.GetConfigValue(key);

            // Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }

        [Test]
        public void GetConfigValue_ThrowsException_WhenValueIsMissing()
        {
            // Arrange
            var key = "Some:MissingKey";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _configHelper.GetConfigValue(key));
            Assert.That(ex.Message, Is.EqualTo($"Configuration value for '{key}' is missing."));
        }
    }
}