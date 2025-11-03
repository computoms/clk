using clk.Domain;
using clk.Domain.Reports;
using clk.Utils;
using FluentAssertions;
using Moq;
using Xunit.Sdk;

namespace clk.unittests;

public class CommandProcessorTests
{
    private readonly Mock<IRecordRepository> _repository = new();
    private readonly Mock<IDisplay> _display = new();
    private readonly Mock<ITimeProvider> _timeProvider = new();
    private readonly Commands.CommandUtils _cmdUtils;

    public CommandProcessorTests()
    {
        _cmdUtils = new Commands.CommandUtils(_repository.Object, _display.Object);
    }

    [Theory]
    [InlineData(new string[4] { "add", "bla", "bli", "blou" }, "bla bli blou", 0, 0)]
    [InlineData(new string[1] { "add" }, "Started empty task", 0, 0)]
    [InlineData(new string[5] { "add", "--at", "10:00", "Test", "#tag" }, "Test #tag", 10, 0)]
    [InlineData(new string[5] { "add", "Test", "--at", "10:00", "#tag" }, "Test #tag", 10, 0)]
    [InlineData(new string[4] { "add", "--at", "10:00", ".123" }, ".123", 10, 0)]
    [InlineData(new string[7] { "add", "This", "is", "a", "test", "/project/task/subtask", ".123" }, "This is a test /project/task/subtask .123", 0, 0)]
    public void WithAddCommand_WhenExecute_ThenAddsRawEntry(string[] arguments, string expectedTitle, int expectedHour, int expectedMin)
    {
        // Arrange
        var settings = new Settings(new ProgramArguments(arguments));
        settings.Data = new Settings.SettingsData();
        var processor = new CommandProcessor(
            new Commands.AddCommand(new ProgramArguments(arguments), settings, _repository.Object, _cmdUtils, _timeProvider.Object));
        if (expectedHour == 0 && expectedMin == 0)
        {
            expectedHour = DateTime.Now.Hour;
            expectedMin = DateTime.Now.Minute;
        }
        TaskLine? addedTask = null;

        _repository.Setup(x => x.AddTask(It.IsAny<TaskLine>()))
            .Callback((TaskLine t) => { addedTask = t; });

        // Act
        processor.Execute();

        // Assert
        addedTask.Should().NotBeNull();
        addedTask.Title.Should().Be(expectedTitle);
        addedTask.StartTime.Hour.Should().Be(expectedHour);
        addedTask.StartTime.Minute.Should().Be(expectedMin);
    }

    [Theory]
    [InlineData(new string[1] { "stop" }, 0, 0)]
    [InlineData(new string[3] { "stop", "--at", "10:00" }, 10, 0)]
    public void WithStopCommand_WhenExecute_ThenAddsRawEntry(string[] args, int expectedHour, int expectedMin)
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.StopCommand(new ProgramArguments(args), _repository.Object, _cmdUtils));
        if (expectedHour == 0 && expectedMin == 0)
        {
            expectedHour = DateTime.Now.Hour;
            expectedMin = DateTime.Now.Minute;
        }

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddTask(It.Is<TaskLine>(t => t.Title == "[Stop]" && t.StartTime.Hour == expectedHour && t.StartTime.Minute == expectedMin)));
    }

    [Fact]
    public void WithNoActivity_WhenRestart_ThenShowsError()
    {
        // Arrange
        _repository.Setup(x => x.GetAll()).Returns(new List<TaskLine>());
        var processor = new CommandProcessor(new Commands.RestartCommand(_repository.Object, _display.Object, _timeProvider.Object, _cmdUtils));

        // Act
        processor.Execute();

        // Assert
        _display.Verify(x => x.Error("No activities to restart"), Times.Once);
    }

    [Fact]
    public void WithOneActivity_WithRestart_WhenExecute_ThenRestartsLastActivity()
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.RestartCommand(_repository.Object, _display.Object, _timeProvider.Object, _cmdUtils));
        _repository.Setup(x => x.GetAll()).Returns(new List<TaskLine>() { new TaskLine("00:00 Test") });

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddTask(It.Is<TaskLine>(t => t.Title == "Test")), Times.Once);
    }
}

