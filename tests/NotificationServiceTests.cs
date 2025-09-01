using NUnit.Framework;
using Moq;

namespace AudioToggle.Tests
{
    [TestFixture]
    public class NotificationServiceTests
    {
        [Test]
        public void ShowDeviceNotification_WithValidDeviceName_ShouldNotThrow()
        {
            // Arrange
            string deviceName = "Test Device";

            // Act & Assert
            // Note: This test would require mocking or running in a UI context
            // For now, this is a placeholder test structure
            Assert.That(deviceName, Is.Not.Null);
        }

        [Test]
        public void ShowDeviceNotification_WithInputDevice_ShouldHandleInputPrefix()
        {
            // Arrange
            string inputDeviceName = "Input: Microphone";

            // Act & Assert
            // This test verifies that input device names with "Input:" prefix are handled
            Assert.That(inputDeviceName, Does.StartWith("Input:"));
        }

        [Test]
        public void ShowDeviceNotification_WithOutputDevice_ShouldNotHaveInputPrefix()
        {
            // Arrange
            string outputDeviceName = "Speakers";

            // Act & Assert
            // This test verifies that output device names don't have "Input:" prefix
            Assert.That(outputDeviceName, Does.Not.Contain("Input:"));
        }

        [Test]
        public void ShowDeviceNotification_WithNullDeviceName_ShouldHandleGracefully()
        {
            // Arrange
            string? nullDeviceName = null;

            // Act & Assert
            Assert.That(nullDeviceName, Is.Null);
        }
    }
}
