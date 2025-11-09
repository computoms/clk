using System.Runtime.CompilerServices;
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
        addedTask.Line.Trim().Should().Be(expectedTitle);
        addedTask.StartTime.Hour.Should().Be(expectedHour);
        addedTask.StartTime.Minute.Should().Be(expectedMin);
    }

    [Fact]
    public void WithAlreadyExistingId_WhenAddCommand_ThenShowsError()
    {
        // Arrange
        var processor = new CommandProcessor(
            new Commands.AddCommand(new ProgramArguments(["add", "Test task", ".123",]), new Settings(new ProgramArguments([])), _repository.Object, _cmdUtils, _timeProvider.Object));
        _repository.Setup(x => x.GetAll()).Returns(new List<TaskLine>()
        {
            new TaskLine("10:00 Existing task .123"),
        });

        // Act
        var act = () => processor.Execute();

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void WithOneActiveTask_WhenCurrent_ThenDisplaysIt()
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.CurrentTaskCommand(_repository.Object, _display.Object));
        _repository.Setup(x => x.FilterByQuery(It.IsAny<RepositoryQuery>())).Returns(new List<TaskLine>()
        {
            new TaskLine("10:00 Test task #tag /path .123"),
        });

        // Act
        processor.Execute();

        // Assert
        _display.Verify(x => x.Print(It.Is<FormattedLine>(l => l.Chunks.Any(c => c.RawText == "10:00 Test task #tag /path .123"))), Times.Once);
    }

    [Fact]
    public void WithMultipleSameTasks_WhenListing_ThenDisplaysDeduplicatedTasks()
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.ListCommand(_repository.Object, _display.Object, new ProgramArguments([])));
        _repository.Setup(x => x.GetAll()).Returns(
        [
            new TaskLine("10:00 Task A #tag1 /path1 .id1"),
            new TaskLine("11:00 Task A #tag1 /path1 .id1"),
            new TaskLine("12:00 Task B #tag2 /path2 .id2"),
        ]);
        var textDispalyed = new List<string>();
        _display.Setup(x => x.Print(It.IsAny<List<FormattedLine>>()))
            .Callback((IEnumerable<FormattedLine> lines) =>
            {
                textDispalyed.AddRange(lines.Select(l => l.Chunks.Aggregate("", (r, t) => r + t.RawText)));
            });

        // Act
        processor.Execute();

        // Assert
        textDispalyed.Count.Should().Be(2);
        textDispalyed[0].Should().Contain("Task A /path1 #tag1 .id1");
        textDispalyed[1].Should().Contain("Task B /path2 #tag2 .id2");
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
    public void WithTwoActivities_WhenSwitch_ThenSwitchesToPreviousActivity()
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.SwitchCommand(_repository.Object, _display.Object, _timeProvider.Object, _cmdUtils));
        _repository.Setup(x => x.FilterByQuery(It.IsAny<RepositoryQuery>())).Returns(new List<TaskLine>()
        {
            new TaskLine("00:00 Test1"),
            new TaskLine("01:00 Test2"),
        });

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddTask(It.Is<TaskLine>(t => t.Title == "Test1")), Times.Once);
    }
}

