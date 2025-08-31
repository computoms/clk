using clk.Domain;
using clk.Domain.Reports;
using clk.Utils;
using FluentAssertions;
using Moq;

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
    [InlineData(new string[4] { "add", "bla", "bli", "blou" }, "bla bli blou", new string[0], new string[0], "", 0, 0)]
    [InlineData(new string[1] { "add" }, "Started empty task", new string[0], new string[0], "", 0, 0)]
    [InlineData(new string[5] { "add", "--at", "10:00", "Test", "+tag" }, "Test", new string[0], new string[1] { "tag" }, "", 10, 0)]
    [InlineData(new string[5] { "add", "Test", "--at", "10:00", "+tag" }, "Test", new string[0], new string[1] { "tag" }, "", 10, 0)]
    [InlineData(new string[4] { "add", "--at", "10:00", ".123" }, "", new string[0], new string[0], "123", 10, 0)]
    [InlineData(new string[7] { "add", "This", "is", "a", "test", "/project/task/subtask", ".123" }, "This is a test", new string[3] { "project", "task", "subtask" }, new string[0], "123", 0, 0)]
    public void WithAddCommand_WhenExecute_ThenAddsRawEntry(string[] arguments, string expectedTitle, string[] expectedTree, string[] expectedTag, string expectedId, int expectedHour, int expectedMin)
    {
        // Arrange
        var settings = new Settings(new ProgramArguments(arguments));
        settings.Data = new Settings.SettingsData();
        var processor = new CommandProcessor(
            new Commands.AddCommand(new ProgramArguments(arguments), settings, _repository.Object, _cmdUtils));
        var expectedTask = new Domain.Task(expectedTitle, [], expectedTag, expectedId);
        if (expectedHour == 0 && expectedMin == 0)
        {
            expectedHour = DateTime.Now.Hour;
            expectedMin = DateTime.Now.Minute;
        }

        Domain.Task? addedTask = null;
        Domain.Record? addedRecord = null;
        _repository.Setup(x => x.AddRecord(It.IsAny<Domain.Task>(), It.IsAny<Domain.Record>()))
            .Callback((Domain.Task t, Domain.Record r) => { addedTask = t; addedRecord = r; });

        // Act
        processor.Execute();

        // Assert
        addedTask.Should().NotBeNull();
        addedRecord.Should().NotBeNull();
        addedTask.Title.Should().Be(expectedTitle);
        addedTask.Id.Should().Be(expectedId);
        addedTask.Tree.Should().HaveCount(expectedTree.Length)
            .And.ContainInOrder(expectedTree);
        addedTask.Tags.Should().HaveCount(expectedTag.Length)
            .And.ContainInOrder(expectedTag);
        addedRecord.StartTime.Hour.Should().Be(expectedHour);
        addedRecord.StartTime.Minute.Should().Be(expectedMin);
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
        _repository.Verify(x => x.AddRecord(It.Is<Domain.Task>(t => t.Title == "[Stop]"), It.Is<Domain.Record>(r => r.StartTime.Hour == expectedHour && r.StartTime.Minute == expectedMin)));
    }

    [Fact]
    public void WithNoActivity_WhenRestart_ThenShowsError()
    {
        // Arrange
        _repository.Setup(x => x.GetAll()).Returns(new List<Activity>());
        var processor = new CommandProcessor(new Commands.RestartCommand(_repository.Object, _display.Object, _timeProvider.Object));

        // Act
        processor.Execute();

        // Assert
        _display.Verify(x => x.Error("No activities to restart"), Times.Once);
    }

    [Fact]
    public void WithOneActivity_WithRestart_WhenExecute_ThenRestartsLastActivity()
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.RestartCommand(_repository.Object, _display.Object, _timeProvider.Object));
        var task = new Domain.Task("Activity", [], ["tag"], "123");
        var record = new Domain.Record(new DateTime(2022, 1, 1), null);
        var activity = new Activity(task, new List<Domain.Record>() { record });
        _repository.Setup(x => x.GetAll()).Returns(new List<Activity>() { activity });

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddRecord(task, It.IsAny<Domain.Record>()), Times.Once);
    }

    [Theory]
    [InlineData("--all")]
    [InlineData("-a")]
    public void WithAllOption_WhenShowing_ThenGetsAllActivities(string option)
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.ShowCommand(
            new ProgramArguments(["show", option]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new DetailsReport(_display.Object, _repository.Object, new ProgramArguments(["show", option]), _timeProvider.Object) }));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.GetAll());
    }

    [Theory]
    [InlineData("--week")]
    [InlineData("-t")]
    public void WithWeekOption_WhenShowing_ThenGetsActivitiesOfTheWeek(string option)
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.ShowCommand(
            new ProgramArguments(["show", option]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new WorktimeReport(_display.Object), new DetailsReport(_display.Object, _repository.Object, new ProgramArguments(["show", option]), _timeProvider.Object) }));
        _timeProvider.Setup(x => x.Now).Returns(new DateTime(2022, 10, 20, 10, 0, 0));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.FilterByDate(new DateTime(2022, 10, 17), new DateTime(2022, 10, 21)), Times.Once);
    }

    [Theory]
    [InlineData("--yesterday")]
    [InlineData("-y")]
    public void WithYesterdayOption_WhenShowing_ThenGetsActivitiesOfYesterday(string option)
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.ShowCommand(
            new ProgramArguments(["show", option]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new DetailsReport(_display.Object, _repository.Object, new ProgramArguments(["show", option]), _timeProvider.Object) }));
        _timeProvider.Setup(x => x.Now).Returns(new DateTime(2022, 10, 20, 10, 0, 0));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.FilterByDate(new DateTime(2022, 10, 19)));
    }

    [Fact]
    public void WithoutOptions_WhenShowing_ThenGetsActivitiesFromToday()
    {
        // Arrange
        var processor = new CommandProcessor(new Commands.ShowCommand(
            new ProgramArguments(["show"]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new DetailsReport(_display.Object, _repository.Object, new ProgramArguments(["show"]), _timeProvider.Object) }));
        _timeProvider.Setup(x => x.Now).Returns(new DateTime(2022, 10, 20, 10, 0, 0));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.FilterByDate(new DateTime(2022, 10, 20)));
    }
}

