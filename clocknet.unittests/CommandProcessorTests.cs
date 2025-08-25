using clocknet.Display;
using clocknet.Reports;
using clocknet.Utils;
using Moq;

namespace clocknet.unittests;

public class CommandProcessorTests
{
    private readonly Mock<IRecordRepository> _repository = new();
    private readonly Mock<IDisplay> _display = new();
    private readonly Mock<ITimeProvider> _timeProvider = new();

    public CommandProcessorTests()
    {
    }

    [Theory]
    [InlineData(new string[4] { "add", "bla", "bli", "blou" }, "bla bli blou", false)]
    [InlineData(new string[1] { "add" }, "Started empty task", false)]
    [InlineData(new string[5] { "add", "--at", "10:00", "Test", "+tag" }, "10:00 Test +tag", true)]
    [InlineData(new string[5] { "add", "Test", "--at", "10:00", "+tag" }, "10:00 Test +tag", true)]
    [InlineData(new string[4] { "add", "--at", "10:00", ".123" }, "10:00 .123", true)]
    public void WithAddCommand_WhenExecute_ThenAddsRawEntry(string[] arguments, string expectedRawEntry, bool expectIncludeTime)
    {
        // Arrange
        var processor = new CommandProcessor(new AddCommand(new ProgramArguments(arguments), new Settings(), _repository.Object));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddRaw(expectedRawEntry, expectIncludeTime));
    }

    [Theory]
    [InlineData("[Stop]", false)]
    public void WithStopCommand_WhenExecute_ThenAddsRawEntry(string expectedRawEntry, bool expectIncludeTime)
    {
        // Arrange
        var processor = new CommandProcessor(new StopCommand(new ProgramArguments(["stop"]), _repository.Object));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddRaw(expectedRawEntry, expectIncludeTime));
    }

    [Fact]
    public void WithNoActivity_WhenRestart_ThenShowsError()
    {
        // Arrange
        _repository.Setup(x => x.GetAll()).Returns(new List<Activity>());
        var processor = new CommandProcessor(new RestartCommand(new ProgramArguments(["restart"]), _repository.Object, _display.Object, _timeProvider.Object));

        // Act
        processor.Execute();

        // Assert
        _display.Verify(x => x.Error("No activities to restart"), Times.Once);
    }

    [Fact]
    public void WithOneActivity_WithRestart_WhenExecute_ThenRestartsLastActivity()
    {
        // Arrange
        var processor = new CommandProcessor(new RestartCommand(new ProgramArguments(["restart"]), _repository.Object, _display.Object, _timeProvider.Object));
        var task = new Task("Activity", new string[] { "tag" }, "123");
        var record = new Record(new DateTime(2022, 1, 1), null);
        var activity = new Activity(task, new List<Record>() { record });
        _repository.Setup(x => x.GetAll()).Returns(new List<Activity>() { activity });

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddRecord(task, It.IsAny<Record>()), Times.Once);
    }

    [Theory]
    [InlineData("--all")]
    [InlineData("-a")]
    public void WithAllOption_WhenShowing_ThenGetsAllActivities(string option)
    {
        // Arrange
        var processor = new CommandProcessor(new ShowCommand(
            new ProgramArguments(["show", option]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new DetailsReport(_display.Object) }));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.GetAll());
    }

    [Theory]
    [InlineData("--week")]
    [InlineData("-w")]
    public void WithWeekOption_WhenShowing_ThenGetsActivitiesOfTheWeek(string option)
    {
        // Arrange
        var processor = new CommandProcessor(new ShowCommand(
            new ProgramArguments(["show", option]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new WorktimeReport(_display.Object), new DetailsReport(_display.Object) }));
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
        var processor = new CommandProcessor(new ShowCommand(
            new ProgramArguments(["show", option]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new DetailsReport(_display.Object) }));
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
        var processor = new CommandProcessor(new ShowCommand(
            new ProgramArguments(["show"]), _repository.Object,
            _timeProvider.Object, new List<IReport>() { new DetailsReport(_display.Object) }));
        _timeProvider.Setup(x => x.Now).Returns(new DateTime(2022, 10, 20, 10, 0, 0));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.FilterByDate(new DateTime(2022, 10, 20)));
    }
}

