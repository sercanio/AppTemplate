using AppTemplate.Application.Services.Authentication.Models;
using FluentAssertions;

namespace AppTemplate.Application.Tests.Unit.Services.AuthenticationServiceTests.Models;

[Trait("Category", "Unit")]
public class DeviceInfoUnitTests
{
  #region Constructor Tests

  [Fact]
  public void Constructor_WithAllParameters_ShouldCreateDeviceInfo()
  {
    // Arrange
    var userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";
    var ipAddress = "192.168.1.1";
    var deviceName = "Windows - Chrome";
    var platform = "Windows";
    var browser = "Chrome";

    // Act
    var deviceInfo = new DeviceInfo(userAgent, ipAddress, deviceName, platform, browser);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo.UserAgent.Should().Be(userAgent);
    deviceInfo.IpAddress.Should().Be(ipAddress);
    deviceInfo.DeviceName.Should().Be(deviceName);
    deviceInfo.Platform.Should().Be(platform);
    deviceInfo.Browser.Should().Be(browser);
  }

  [Fact]
  public void Constructor_WithRequiredParametersOnly_ShouldCreateDeviceInfo()
  {
    // Arrange
    var userAgent = "Mozilla/5.0";
    var ipAddress = "10.0.0.1";

    // Act
    var deviceInfo = new DeviceInfo(userAgent, ipAddress);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo.UserAgent.Should().Be(userAgent);
    deviceInfo.IpAddress.Should().Be(ipAddress);
    deviceInfo.DeviceName.Should().BeNull();
    deviceInfo.Platform.Should().BeNull();
    deviceInfo.Browser.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithNullUserAgent_ShouldCreateDeviceInfo()
  {
    // Arrange
    string? userAgent = null;
    var ipAddress = "192.168.0.1";

    // Act
    var deviceInfo = new DeviceInfo(userAgent, ipAddress);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo.UserAgent.Should().BeNull();
    deviceInfo.IpAddress.Should().Be(ipAddress);
  }

  [Fact]
  public void Constructor_WithNullIpAddress_ShouldCreateDeviceInfo()
  {
    // Arrange
    var userAgent = "Mozilla/5.0";
    string? ipAddress = null;

    // Act
    var deviceInfo = new DeviceInfo(userAgent, ipAddress);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo.UserAgent.Should().Be(userAgent);
    deviceInfo.IpAddress.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithAllNullParameters_ShouldCreateDeviceInfo()
  {
    // Act
    var deviceInfo = new DeviceInfo(null, null, null, null, null);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo.UserAgent.Should().BeNull();
    deviceInfo.IpAddress.Should().BeNull();
    deviceInfo.DeviceName.Should().BeNull();
    deviceInfo.Platform.Should().BeNull();
    deviceInfo.Browser.Should().BeNull();
  }

  [Fact]
  public void Constructor_WithEmptyStrings_ShouldCreateDeviceInfo()
  {
    // Act
    var deviceInfo = new DeviceInfo(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    // Assert
    deviceInfo.Should().NotBeNull();
    deviceInfo.UserAgent.Should().BeEmpty();
    deviceInfo.IpAddress.Should().BeEmpty();
    deviceInfo.DeviceName.Should().BeEmpty();
    deviceInfo.Platform.Should().BeEmpty();
    deviceInfo.Browser.Should().BeEmpty();
  }

  #endregion

  #region Equality Tests

  [Fact]
  public void Equals_SameValues_ShouldReturnTrue()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");

    // Act & Assert
    deviceInfo1.Should().Be(deviceInfo2);
    (deviceInfo1 == deviceInfo2).Should().BeTrue();
    (deviceInfo1 != deviceInfo2).Should().BeFalse();
  }

  [Fact]
  public void Equals_DifferentUserAgent_ShouldReturnFalse()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Different/Agent", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");

    // Act & Assert
    deviceInfo1.Should().NotBe(deviceInfo2);
    (deviceInfo1 == deviceInfo2).Should().BeFalse();
    (deviceInfo1 != deviceInfo2).Should().BeTrue();
  }

  [Fact]
  public void Equals_DifferentIpAddress_ShouldReturnFalse()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Mozilla/5.0", "10.0.0.1", "Windows - Chrome", "Windows", "Chrome");

    // Act & Assert
    deviceInfo1.Should().NotBe(deviceInfo2);
  }

  [Fact]
  public void Equals_DifferentDeviceName_ShouldReturnFalse()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Mac - Safari", "Windows", "Chrome");

    // Act & Assert
    deviceInfo1.Should().NotBe(deviceInfo2);
  }

  [Fact]
  public void Equals_DifferentPlatform_ShouldReturnFalse()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Linux", "Chrome");

    // Act & Assert
    deviceInfo1.Should().NotBe(deviceInfo2);
  }

  [Fact]
  public void Equals_DifferentBrowser_ShouldReturnFalse()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Firefox");

    // Act & Assert
    deviceInfo1.Should().NotBe(deviceInfo2);
  }

  [Fact]
  public void Equals_BothWithNullValues_ShouldReturnTrue()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo(null, null, null, null, null);
    var deviceInfo2 = new DeviceInfo(null, null, null, null, null);

    // Act & Assert
    deviceInfo1.Should().Be(deviceInfo2);
  }

  [Fact]
  public void Equals_OneNullOneValue_ShouldReturnFalse()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1");
    var deviceInfo2 = new DeviceInfo(null, null);

    // Act & Assert
    deviceInfo1.Should().NotBe(deviceInfo2);
  }

  #endregion

  #region GetHashCode Tests

  [Fact]
  public void GetHashCode_SameValues_ShouldReturnSameHashCode()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");

    // Act
    var hash1 = deviceInfo1.GetHashCode();
    var hash2 = deviceInfo2.GetHashCode();

    // Assert
    hash1.Should().Be(hash2);
  }

  [Fact]
  public void GetHashCode_DifferentValues_ShouldReturnDifferentHashCode()
  {
    // Arrange
    var deviceInfo1 = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");
    var deviceInfo2 = new DeviceInfo("Different/Agent", "10.0.0.1", "Mac - Safari", "macOS", "Safari");

    // Act
    var hash1 = deviceInfo1.GetHashCode();
    var hash2 = deviceInfo2.GetHashCode();

    // Assert
    hash1.Should().NotBe(hash2);
  }

  [Fact]
  public void GetHashCode_WithNullValues_ShouldNotThrow()
  {
    // Arrange
    var deviceInfo = new DeviceInfo(null, null, null, null, null);

    // Act
    var action = () => deviceInfo.GetHashCode();

    // Assert
    action.Should().NotThrow();
  }

  #endregion

  #region ToString Tests

  [Fact]
  public void ToString_WithAllValues_ShouldReturnFormattedString()
  {
    // Arrange
    var deviceInfo = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");

    // Act
    var result = deviceInfo.ToString();

    // Assert
    result.Should().NotBeNullOrEmpty();
    result.Should().Contain("Mozilla/5.0");
    result.Should().Contain("192.168.1.1");
    result.Should().Contain("Windows - Chrome");
    result.Should().Contain("Windows");
    result.Should().Contain("Chrome");
  }

  [Fact]
  public void ToString_WithNullValues_ShouldNotThrow()
  {
    // Arrange
    var deviceInfo = new DeviceInfo(null, null, null, null, null);

    // Act
    var action = () => deviceInfo.ToString();

    // Assert
    action.Should().NotThrow();
  }

  [Fact]
  public void ToString_WithRequiredParametersOnly_ShouldReturnFormattedString()
  {
    // Arrange
    var deviceInfo = new DeviceInfo("Mozilla/5.0", "192.168.1.1");

    // Act
    var result = deviceInfo.ToString();

    // Assert
    result.Should().NotBeNullOrEmpty();
    result.Should().Contain("Mozilla/5.0");
    result.Should().Contain("192.168.1.1");
  }

  #endregion

  #region Real-World Scenarios

  [Fact]
  public void DeviceInfo_WindowsChromeScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "192.168.1.100",
        "Windows - Chrome",
        "Windows",
        "Chrome");

    // Assert
    deviceInfo.UserAgent.Should().Contain("Windows NT");
    deviceInfo.IpAddress.Should().Be("192.168.1.100");
    deviceInfo.DeviceName.Should().Be("Windows - Chrome");
    deviceInfo.Platform.Should().Be("Windows");
    deviceInfo.Browser.Should().Be("Chrome");
  }

  [Fact]
  public void DeviceInfo_MacOSSafariScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.0 Safari/605.1.15",
        "10.0.0.50",
        "macOS - Safari",
        "macOS",
        "Safari");

    // Assert
    deviceInfo.Platform.Should().Be("macOS");
    deviceInfo.Browser.Should().Be("Safari");
    deviceInfo.IpAddress.Should().Be("10.0.0.50");
  }

  [Fact]
  public void DeviceInfo_AndroidChromeScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (Linux; Android 13) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.6099.144 Mobile Safari/537.36",
        "172.16.0.10",
        "Android - Chrome",
        "Android",
        "Chrome");

    // Assert
    deviceInfo.Platform.Should().Be("Android");
    deviceInfo.Browser.Should().Be("Chrome");
    deviceInfo.DeviceName.Should().Be("Android - Chrome");
  }

  [Fact]
  public void DeviceInfo_IOSSafariScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1",
        "192.168.100.5",
        "iOS - Safari",
        "iOS",
        "Safari");

    // Assert
    deviceInfo.Platform.Should().Be("iOS");
    deviceInfo.Browser.Should().Be("Safari");
    deviceInfo.IpAddress.Should().Be("192.168.100.5");
  }

  [Fact]
  public void DeviceInfo_LinuxFirefoxScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (X11; Linux x86_64; rv:120.0) Gecko/20100101 Firefox/120.0",
        "172.20.10.2",
        "Linux - Firefox",
        "Linux",
        "Firefox");

    // Assert
    deviceInfo.Platform.Should().Be("Linux");
    deviceInfo.Browser.Should().Be("Firefox");
  }

  [Fact]
  public void DeviceInfo_EdgeBrowserScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36 Edg/120.0.0.0",
        "192.168.1.200",
        "Windows - Edge",
        "Windows",
        "Edge");

    // Assert
    deviceInfo.Browser.Should().Be("Edge");
    deviceInfo.Platform.Should().Be("Windows");
  }

  [Fact]
  public void DeviceInfo_BraveBrowserScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "192.168.1.150",
        "Windows - Brave",
        "Windows",
        "Brave");

    // Assert
    deviceInfo.Browser.Should().Be("Brave");
  }

  [Fact]
  public void DeviceInfo_UnknownBrowserScenario_ShouldCreateCorrectly()
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo(
        string.Empty,
        "192.168.1.1",
        "Unknown Device",
        "Unknown",
        "Unknown");

    // Assert
    deviceInfo.Platform.Should().Be("Unknown");
    deviceInfo.Browser.Should().Be("Unknown");
    deviceInfo.DeviceName.Should().Be("Unknown Device");
  }

  #endregion

  #region IP Address Scenarios

  [Theory]
  [InlineData("192.168.1.1", "192.168.1.1")]
  [InlineData("10.0.0.1", "10.0.0.1")]
  [InlineData("172.16.0.1", "172.16.0.1")]
  [InlineData("2001:0db8:85a3::8a2e:0370:7334", "2001:0db8:85a3::8a2e:0370:7334")] // IPv6
  [InlineData("::1", "::1")] // IPv6 loopback
  [InlineData("127.0.0.1", "127.0.0.1")] // IPv4 loopback
  public void DeviceInfo_WithVariousIpAddresses_ShouldPreserveIpAddress(string inputIp, string expectedIp)
  {
    // Arrange & Act
    var deviceInfo = new DeviceInfo("Mozilla/5.0", inputIp);

    // Assert
    deviceInfo.IpAddress.Should().Be(expectedIp);
  }

  #endregion

  #region Property Access Tests

  [Fact]
  public void DeviceInfo_Properties_ShouldBeReadable()
  {
    // Arrange
    var deviceInfo = new DeviceInfo(
        "TestAgent",
        "TestIP",
        "TestDevice",
        "TestPlatform",
        "TestBrowser");

    // Act & Assert - Verify all properties are accessible
    var userAgent = deviceInfo.UserAgent;
    var ipAddress = deviceInfo.IpAddress;
    var deviceName = deviceInfo.DeviceName;
    var platform = deviceInfo.Platform;
    var browser = deviceInfo.Browser;

    userAgent.Should().Be("TestAgent");
    ipAddress.Should().Be("TestIP");
    deviceName.Should().Be("TestDevice");
    platform.Should().Be("TestPlatform");
    browser.Should().Be("TestBrowser");
  }

  [Fact]
  public void DeviceInfo_WithLongStrings_ShouldHandleCorrectly()
  {
    // Arrange
    var longUserAgent = new string('A', 1000);
    var longIpAddress = new string('1', 100);
    var longDeviceName = new string('B', 500);

    // Act
    var deviceInfo = new DeviceInfo(longUserAgent, longIpAddress, longDeviceName, "Platform", "Browser");

    // Assert
    deviceInfo.UserAgent.Should().HaveLength(1000);
    deviceInfo.IpAddress.Should().HaveLength(100);
    deviceInfo.DeviceName.Should().HaveLength(500);
  }

  [Fact]
  public void DeviceInfo_WithSpecialCharacters_ShouldPreserveCharacters()
  {
    // Arrange
    var specialUserAgent = "Mozilla/5.0 (特殊文字テスト) 中文测试";
    var specialDeviceName = "Device-Name_With!Special@Characters#123";

    // Act
    var deviceInfo = new DeviceInfo(specialUserAgent, "192.168.1.1", specialDeviceName, "Platform", "Browser");

    // Assert
    deviceInfo.UserAgent.Should().Be(specialUserAgent);
    deviceInfo.DeviceName.Should().Be(specialDeviceName);
  }

  #endregion

  #region Deconstruction Tests

  [Fact]
  public void DeviceInfo_Deconstruction_ShouldWorkCorrectly()
  {
    // Arrange
    var deviceInfo = new DeviceInfo("Mozilla/5.0", "192.168.1.1", "Windows - Chrome", "Windows", "Chrome");

    // Act
    var (userAgent, ipAddress, deviceName, platform, browser) = deviceInfo;

    // Assert
    userAgent.Should().Be("Mozilla/5.0");
    ipAddress.Should().Be("192.168.1.1");
    deviceName.Should().Be("Windows - Chrome");
    platform.Should().Be("Windows");
    browser.Should().Be("Chrome");
  }

  #endregion
}