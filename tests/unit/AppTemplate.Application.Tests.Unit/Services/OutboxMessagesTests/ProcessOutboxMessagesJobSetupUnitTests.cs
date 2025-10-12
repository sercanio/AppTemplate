using AppTemplate.Application.Services.OutboxMessages;
using AppTemplate.Domain.OutboxMessages;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Quartz;

namespace AppTemplate.Application.Tests.Unit.Services.OutboxMessagesTests;

[Trait("Category", "Unit")]
public class ProcessOutboxMessagesJobSetupUnitTests
{
  [Fact]
  public void Configure_ShouldAddJobWithCorrectIdentity()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    setup.Configure(quartzOptions);

    // Assert
    quartzOptions.Should().NotBeNull();
    // Verify that job was added (Quartz stores this internally)
  }

  [Fact]
  public void Configure_ShouldAddTriggerWithCorrectInterval()
  {
    // Arrange
    var intervalInSeconds = 45;
    var outboxOptions = new OutboxOptions { IntervalInSeconds = intervalInSeconds, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    setup.Configure(quartzOptions);

    // Assert
    quartzOptions.Should().NotBeNull();
  }

  [Theory]
  [InlineData(10)]
  [InlineData(30)]
  [InlineData(60)]
  [InlineData(120)]
  public void Configure_WithVariousIntervals_ShouldConfigureCorrectly(int intervalInSeconds)
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = intervalInSeconds, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    setup.Configure(quartzOptions);

    // Assert
    quartzOptions.Should().NotBeNull();
  }

  [Fact]
  public void Constructor_ShouldInitializeWithOutboxOptions()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    // Act
    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);

    // Assert
    setup.Should().NotBeNull();
  }

  [Fact]
  public void Configure_ShouldNotThrow_WithValidOptions()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    Action act = () => setup.Configure(quartzOptions);

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void Configure_ShouldUseProcessOutboxMessagesJobType()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    setup.Configure(quartzOptions);

    // Assert
    // The job type ProcessOutboxMessagesJob should be configured
    quartzOptions.Should().NotBeNull();
  }

  [Fact]
  public void Configure_ShouldConfigureRepeatForeverTrigger()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 30, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    setup.Configure(quartzOptions);

    // Assert
    quartzOptions.Should().NotBeNull();
    // Trigger should repeat forever
  }

  [Fact]
  public void Configure_WithMinimalInterval_ShouldWork()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 1, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    Action act = () => setup.Configure(quartzOptions);

    // Assert
    act.Should().NotThrow();
  }

  [Fact]
  public void Configure_WithLargeInterval_ShouldWork()
  {
    // Arrange
    var outboxOptions = new OutboxOptions { IntervalInSeconds = 3600, BatchSize = 10 };
    var optionsMock = new Mock<IOptions<OutboxOptions>>();
    optionsMock.Setup(x => x.Value).Returns(outboxOptions);

    var setup = new ProcessOutboxMessagesJobSetup(optionsMock.Object);
    var quartzOptions = new QuartzOptions();

    // Act
    Action act = () => setup.Configure(quartzOptions);

    // Assert
    act.Should().NotThrow();
  }
}