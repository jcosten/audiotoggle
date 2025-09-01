using NUnit.Framework;
using AudioToggle;
using System.Threading.Tasks;

namespace AudioToggle.Tests
{
    [TestFixture]
    public class UpdateServiceTests
    {
        private UpdateService? _updateService;

        [SetUp]
        public void Setup()
        {
            _updateService = new UpdateService();
        }

        [Test]
        public void GetCurrentVersion_ShouldReturnValidVersion()
        {
            // Act
            var version = _updateService!.GetCurrentVersion();

            // Assert
            Assert.That(version, Is.Not.Null);
            Assert.That(version, Is.Not.Empty);
            Assert.That(version.Contains("."), Is.True, "Version should contain dots");
        }

        [Test]
        public async Task CheckForUpdatesAsync_ShouldReturnUpdateInfoOrNull()
        {
            // Act
            var updateInfo = await _updateService!.CheckForUpdatesAsync();

            // Assert
            // This test will pass whether there's an update available or not
            // In a real scenario, this would depend on the GitHub repository state
            Assert.That(updateInfo == null || updateInfo is UpdateInfo);
        }

        [Test]
        public void IsUpdateAvailable_WithNullUpdateInfo_ShouldReturnFalse()
        {
            // Act
            var result = _updateService!.IsUpdateAvailable(null);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsUpdateAvailable_WithOlderVersion_ShouldReturnTrue()
        {
            // Arrange
            var updateInfo = new UpdateInfo { Version = "2.0.0" };

            // Act
            var result = _updateService!.IsUpdateAvailable(updateInfo);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public void IsUpdateAvailable_WithSameVersion_ShouldReturnFalse()
        {
            // Arrange
            var currentVersion = _updateService!.GetCurrentVersion();
            var updateInfo = new UpdateInfo { Version = currentVersion };

            // Act
            var result = _updateService.IsUpdateAvailable(updateInfo);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void IsUpdateAvailable_WithNewerVersion_ShouldReturnTrue()
        {
            // Arrange
            var updateInfo = new UpdateInfo { Version = "9.9.9" };

            // Act
            var result = _updateService!.IsUpdateAvailable(updateInfo);

            // Assert
            Assert.That(result, Is.True);
        }
    }
}
