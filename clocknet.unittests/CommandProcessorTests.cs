using clocknet.Display;
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
    [InlineData(new string[4] { "add", "bla", "bli", "blou" }, "bla bli blou")]
    [InlineData(new string[1] { "add" }, "Started empty task")]
    [InlineData(new string[1] { "stop" }, "[Stop]")]
    [InlineData(new string[5] { "add", "--at", "10:00", "Test", "+tag" }, "10:00 Test +tag")]
    [InlineData(new string[5] { "add", "Test", "--at", "10:00", "+tag" }, "10:00 Test +tag")]
    public void WithBasicCommand_WhenExecute_ThenAddsRawEntry(string[] arugments, string expectedRawEntry)
    {
        // Arrange
        var processor = SetupProcessor(arugments);

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.AddRaw(expectedRawEntry));
    }

    [Fact]
    public void WithNoActivity_WhenRestart_ThenShowsError()
    {
        // Arrange
        _repository.Setup(x => x.GetAll()).Returns(new List<Activity>());
        var processor = SetupProcessor("restart");

        // Act
        processor.Execute();

        // Assert
        _display.Verify(x => x.Error("No activities to restart"), Times.Once);
    }

    [Fact]
    public void WithOneActivity_WithRestart_WhenExecute_ThenRestartsLastActivity()
    {
        // Arrange
        var processor = SetupProcessor("restart");
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
        var processor = SetupProcessor("show", option);

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
        var processor = SetupProcessor("show", option);
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
        var processor = SetupProcessor("show", option);
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
        var processor = SetupProcessor("show");
        _timeProvider.Setup(x => x.Now).Returns(new DateTime(2022, 10, 20, 10, 0, 0));

        // Act
        processor.Execute();

        // Assert
        _repository.Verify(x => x.FilterByDate(new DateTime(2022, 10, 20)));
    }

    private CommandProcessor SetupProcessor(params string[] arguments)
    {
        return new CommandProcessor(arguments, _repository.Object, _display.Object, _timeProvider.Object, new Settings());
    }
}

