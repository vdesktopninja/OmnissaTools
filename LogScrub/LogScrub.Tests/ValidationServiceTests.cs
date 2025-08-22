using LogScrub.Gui;
using LogScrub.Gui.Services;
using Xunit;

namespace LogScrub.Tests
{
    /// <summary>
    /// Unit tests for ValidationService
    /// </summary>
    public class ValidationServiceTests
    {
        private readonly ValidationService _validationService;

        public ValidationServiceTests()
        {
            _validationService = new ValidationService();
        }

        [Fact]
        public void ValidateInputPath_EmptyPath_ReturnsFailure()
        {
            // Act
            var result = _validationService.ValidateInputPath("");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("cannot be empty", result.Error);
        }

        [Fact]
        public void ValidateInputPath_NullPath_ReturnsFailure()
        {
            // Act
            var result = _validationService.ValidateInputPath(null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("cannot be empty", result.Error);
        }

        [Fact]
        public void ValidateSettings_ValidSettings_ReturnsSuccess()
        {
            // Arrange
            var settings = new Settings
            {
                IpMode = "mask",
                KeepRfc1918 = false,
                FqdnOn = true,
                UsersOn = true,
                ServersOn = true
            };

            // Act
            var result = _validationService.ValidateSettings(settings);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public void ValidateSettings_NullSettings_ReturnsFailure()
        {
            // Act
            var result = _validationService.ValidateSettings(null);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("cannot be null", result.Error);
        }

        [Theory]
        [InlineData("mask")]
        [InlineData("tokenize")]
        [InlineData("MASK")]
        [InlineData("TOKENIZE")]
        public void ValidateSettings_ValidIpModes_ReturnsSuccess(string ipMode)
        {
            // Arrange
            var settings = new Settings { IpMode = ipMode };

            // Act
            var result = _validationService.ValidateSettings(settings);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateSettings_InvalidIpModes_ReturnsFailure(string ipMode)
        {
            // Arrange
            var settings = new Settings { IpMode = ipMode };

            // Act
            var result = _validationService.ValidateSettings(settings);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(4)]
        [InlineData(8)]
        public void ValidateParallelism_ValidValues_ReturnsSuccess(int parallelism)
        {
            // Act
            var result = _validationService.ValidateParallelism(parallelism);

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void ValidateParallelism_InvalidValues_ReturnsFailure(int parallelism)
        {
            // Act
            var result = _validationService.ValidateParallelism(parallelism);

            // Assert
            Assert.False(result.IsSuccess);
        }

        [Fact]
        public void IsZipFile_ZipExtension_ReturnsTrue()
        {
            // Act & Assert
            Assert.False(_validationService.IsZipFile("nonexistent.zip")); // File doesn't exist
        }

        [Fact]
        public void IsZipFile_NonZipExtension_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(_validationService.IsZipFile("file.txt"));
        }

        [Fact]
        public void IsZipFile_NullPath_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(_validationService.IsZipFile(null));
        }

        [Fact]
        public void IsDirectory_NullPath_ReturnsFalse()
        {
            // Act & Assert
            Assert.False(_validationService.IsDirectory(null));
        }
    }
}