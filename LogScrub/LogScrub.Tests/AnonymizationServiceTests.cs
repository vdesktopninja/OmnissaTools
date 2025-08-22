using LogScrub.Gui;
using LogScrub.Gui.Services;
using Xunit;

namespace LogScrub.Tests
{
    /// <summary>
    /// Unit tests for AnonymizationService
    /// </summary>
    public class AnonymizationServiceTests
    {
        private readonly AnonymizationService _anonymizationService;
        private readonly Settings _defaultSettings;

        public AnonymizationServiceTests()
        {
            _anonymizationService = new AnonymizationService();
            _defaultSettings = new Settings
            {
                IpMode = "mask",
                KeepRfc1918 = false,
                FqdnOn = true,
                UsersOn = true,
                ServersOn = true
            };
        }

        [Fact]
        public void CreateAnonymizer_ValidParameters_ReturnsAnonymizer()
        {
            // Act
            var anonymizer = _anonymizationService.CreateAnonymizer("test-secret", _defaultSettings);

            // Assert
            Assert.NotNull(anonymizer);
        }

        [Fact]
        public void AnonymizeLine_SimpleText_ReturnsOriginalText()
        {
            // Arrange
            var anonymizer = _anonymizationService.CreateAnonymizer("test-secret", new Settings
            {
                IpMode = "mask",
                KeepRfc1918 = false,
                FqdnOn = false,
                UsersOn = false,
                ServersOn = false
            });

            // Act
            var result = _anonymizationService.AnonymizeLine(anonymizer, "This is a simple log entry");

            // Assert
            Assert.Equal("This is a simple log entry", result.Line);
            Assert.Equal(0, result.IpMatches);
            Assert.Equal(0, result.FqdnMatches);
            Assert.Equal(0, result.UserMatches);
            Assert.Equal(0, result.ServerMatches);
        }

        [Fact]
        public void AnonymizeLine_ContainsIP_AnonymizesCorrectly()
        {
            // Arrange
            var anonymizer = _anonymizationService.CreateAnonymizer("test-secret", _defaultSettings);
            var testLine = "Connection from 192.168.1.100 established";

            // Act
            var result = _anonymizationService.AnonymizeLine(anonymizer, testLine);

            // Assert
            Assert.NotEqual(testLine, result.Line); // Should be modified
            Assert.Contains("192.***.***.100", result.Line); // Should be masked
            Assert.Equal(1, result.IpMatches);
        }

        [Fact]
        public void AnonymizeLine_ContainsEmail_AnonymizesCorrectly()
        {
            // Arrange
            var anonymizer = _anonymizationService.CreateAnonymizer("test-secret", _defaultSettings);
            var testLine = "User john.doe@example.com logged in";

            // Act
            var result = _anonymizationService.AnonymizeLine(anonymizer, testLine);

            // Assert
            Assert.NotEqual(testLine, result.Line); // Should be modified
            Assert.DoesNotContain("john.doe@example.com", result.Line); // Original email should be removed
            Assert.Equal(1, result.UserMatches);
        }

        [Fact]
        public void AnonymizeLine_DisabledFeatures_NoAnonymization()
        {
            // Arrange
            var settings = new Settings
            {
                IpMode = "mask",
                KeepRfc1918 = false,
                FqdnOn = false,
                UsersOn = false,
                ServersOn = false
            };
            var anonymizer = _anonymizationService.CreateAnonymizer("test-secret", settings);
            var testLine = "User john.doe@example.com from server.example.com at 192.168.1.100";

            // Act
            var result = _anonymizationService.AnonymizeLine(anonymizer, testLine);

            // Assert
            // Only IP should be anonymized since other features are disabled
            Assert.Equal(1, result.IpMatches);
            Assert.Equal(0, result.FqdnMatches);
            Assert.Equal(0, result.UserMatches);
            Assert.Equal(0, result.ServerMatches);
        }

        [Theory]
        [InlineData("192.168.1.1")] // Private IP
        [InlineData("10.0.0.1")]    // Private IP
        [InlineData("172.16.0.1")]  // Private IP
        public void AnonymizeLine_PrivateIPWithKeepRfc1918_PreservesIP(string privateIp)
        {
            // Arrange
            var settings = new Settings
            {
                IpMode = "mask",
                KeepRfc1918 = true,
                FqdnOn = false,
                UsersOn = false,
                ServersOn = false
            };
            var anonymizer = _anonymizationService.CreateAnonymizer("test-secret", settings);
            var testLine = $"Connection from {privateIp} established";

            // Act
            var result = _anonymizationService.AnonymizeLine(anonymizer, testLine);

            // Assert
            Assert.Contains(privateIp, result.Line); // Private IP should be preserved
            Assert.Equal(0, result.IpMatches); // Should not count as anonymized
        }
    }
}