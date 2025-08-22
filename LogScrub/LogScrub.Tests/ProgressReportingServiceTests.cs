using LogScrub.Gui.Services;
using Xunit;

namespace LogScrub.Tests
{
    /// <summary>
    /// Unit tests for ProgressReportingService
    /// </summary>
    public class ProgressReportingServiceTests
    {
        private readonly ProgressReportingService _progressService;

        public ProgressReportingServiceTests()
        {
            _progressService = new ProgressReportingService();
        }

        [Theory]
        [InlineData(0, "0 B")]
        [InlineData(512, "512 B")]
        [InlineData(1024, "1 KB")]
        [InlineData(1536, "1.5 KB")]
        [InlineData(1048576, "1 MB")]
        [InlineData(1073741824, "1 GB")]
        public void FormatBytes_VariousSizes_FormatsCorrectly(long bytes, string expected)
        {
            // Act
            var result = _progressService.FormatBytes(bytes);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0, 0, "0m 0s")]
        [InlineData(30, 0, "0m 30s")]
        [InlineData(90, 0, "1m 30s")]
        [InlineData(3661, 0, "1h 1m 1s")]
        public void FormatEta_VariousDurations_FormatsCorrectly(int seconds, int milliseconds, string expected)
        {
            // Arrange
            var timeSpan = TimeSpan.FromSeconds(seconds).Add(TimeSpan.FromMilliseconds(milliseconds));

            // Act
            var result = _progressService.FormatEta(timeSpan);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(0, 100, 0.0)]
        [InlineData(50, 100, 50.0)]
        [InlineData(100, 100, 100.0)]
        [InlineData(150, 100, 100.0)] // Should cap at 100%
        public void CalculateProgress_VariousValues_ReturnsCorrectPercentage(long completed, long total, double expected)
        {
            // Act
            var result = _progressService.CalculateProgress(completed, total);

            // Assert
            Assert.Equal(expected, result, 1); // Allow 1 decimal place tolerance
        }

        [Fact]
        public void CalculateProgress_ZeroTotal_ReturnsZero()
        {
            // Act
            var result = _progressService.CalculateProgress(50, 0);

            // Assert
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void CreateProgressMessage_WithAllParameters_ReturnsFormattedMessage()
        {
            // Act
            var result = _progressService.CreateProgressMessage(1024, 2048, 512, "test.log");

            // Assert
            Assert.Contains("50.0%", result);
            Assert.Contains("1 KB", result);
            Assert.Contains("2 KB", result);
            Assert.Contains("512 B/s", result);
            Assert.Contains("test.log", result);
            Assert.Contains("ETA", result);
        }

        [Fact]
        public void CreateProgressMessage_WithoutCurrentFile_ReturnsFormattedMessage()
        {
            // Act
            var result = _progressService.CreateProgressMessage(1024, 2048, 512, null);

            // Assert
            Assert.Contains("50.0%", result);
            Assert.Contains("1 KB", result);
            Assert.Contains("2 KB", result);
            Assert.Contains("512 B/s", result);
            Assert.DoesNotContain(".log", result);
            Assert.Contains("ETA", result);
        }

        [Fact]
        public void CreateProgressMessage_ZeroSpeed_ShowsZeroETA()
        {
            // Act
            var result = _progressService.CreateProgressMessage(1024, 2048, 0, "test.log");

            // Assert
            Assert.Contains("0 B/s", result);
            Assert.Contains("ETA 0m 0s", result);
        }
    }
}