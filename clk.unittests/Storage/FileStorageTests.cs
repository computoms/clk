using Moq;
using Moq.AutoMock;
using FluentAssertions;
using clk.Utils;
using clk.Domain;
using clk.Infra;

namespace clk.unittests;

public class FileStorageTests
{
    private readonly AutoMocker _mocker = new();
    private readonly FileStorage _storage;
    private readonly Mock<IStream> _stream;
    private readonly Domain.Task _activity;
    private readonly Domain.Record _record;
    private readonly DateTime _baseTime = new DateTime(2022, 10, 10, 11, 12, 0);
    private const string _today = "[2022-10-10]";
    private const string _yesterday = "[2022-10-09]";

    public FileStorageTests()
    {
        _storage = _mocker.CreateInstance<FileStorage>();
        _stream = _mocker.GetMock<IStream>();
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime);
        _activity = new Domain.Task("Test", new string[] { "feature" }, "123");
        _record = new Domain.Record(_baseTime, null);
    }

    #region AddEntry

    [Theory]
    [InlineData("", true)]
    [InlineData(_yesterday, true)]
    [InlineData(_today, false)]
    [InlineData("[InvalidDate]", true)]
    public void WithEmptyFile_WhenAddingEntry_ThenDateAndEntryIsAdded(string existingLine, bool shouldAddToday)
    {
        // Arrange
        SetupLines(existingLine);

        // Act
        _storage.AddEntry(_activity, _record);

        // Assert
        _stream.Verify(x => x.AddLine("[2022-10-10]"), shouldAddToday ? Times.Once : Times.Never);
        _stream.Verify(x => x.AddLine("11:12 Test +feature .123"), Times.Once);
    }

    [Theory]
    [InlineData("Test", new string[1] { "tag" }, "123", "11:12 Test +tag .123")]
    [InlineData("Test", new string[2] { "tag1", "tag2" }, "123", "11:12 Test +tag1 +tag2 .123")]
    [InlineData("Test", new string[0] { }, "123", "11:12 Test .123")]
    [InlineData("Test", new string[1] { "tag" }, "", "11:12 Test +tag")]
    [InlineData("Test", new string[0] { }, "", "11:12 Test")]
    [InlineData("", new string[1] { "tag" }, "", "11:12 +tag")]
    public void WithGivenActivityAndRecord_WhenAdd_ThenAddsCorrectLine(string title, string[] tags, string id, string expectedLine)
    {
        // Arrange
        SetupLines(_today);
        var task = new Domain.Task(title, tags, id);

        // Act
        _storage.AddEntry(task, _record);

        // Assert
        _stream.Verify(x => x.AddLine(expectedLine), Times.Once);
    }

    [Fact]
    public void WithNonUniqueId_WhenAddingEntry_ThenThrowsException()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 .123");

        // Act
        var act = () => _storage.AddEntry(new Domain.Task("Activity2", new string[0], "123"), new Domain.Record(DateTime.Now, null));

        // Assert
        act.Should().Throw<InvalidDataException>("Id 123 already exists");
    }

    [Fact]
    public void WithNonUniqueId_WhenAddingSameEntry_ThenDoesNotThrow()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 .123");

        // Act
        var act = () => _storage.AddEntry(new Domain.Task("Activity1", new string[0], "123"), new Domain.Record(DateTime.Now, null));

        // Assert
        act.Should().NotThrow<InvalidDataException>();
    }

    #endregion AddEntry

    #region GetActivities

    [Fact]
    public void WithEmptyFile_WhenGetAllActivities_ThenReturnsEmtpyCollection()
    {
        // Arrange
        SetupLines();

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().BeEmpty();
    }

    [Theory]
    [InlineData("11:11 This is a test +feature .123", "This is a test", new string[1] { "feature" }, "123")]
    [InlineData("   11:11 This is a test +feature .123   ", "This is a test", new string[1] { "feature" }, "123")]
    [InlineData("11:11 Activity .123", "Activity", new string[0] { }, "123")]
    [InlineData("11:11 Activity +tag", "Activity", new string[1] { "tag" }, "")]
    [InlineData("11:11 Activity +tag1 +tag2", "Activity", new string[2] { "tag1", "tag2" }, "")]
    [InlineData("11:11 +tag1 +tag2 .124", "", new string[2] { "tag1", "tag2" }, "124")]
    [InlineData("11:11 Activity", "Activity", new string[0] { }, "")]
    public void WithOneEntry_WhenGetAllActivities_ThenReturnsOneActivity(string existingLine, string expectedTitle, string[] expectedTags, string expectedId)
    {
        // Arrange
        SetupLines(_today, existingLine);

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities[0].Task.Title.Should().Be(expectedTitle);
        activities[0].Task.Tags.Should().HaveCount(expectedTags.Count());
        foreach (var tag in expectedTags.Zip(activities[0].Task.Tags))
			tag.Second.Should().Be(tag.First);
        activities[0].Task.Id.Should().Be(expectedId);
    }

    [Fact]
    public void WithDuplicateActivity_WhenGetAllActivities_ThenReturnsOneActivity()
    {
        // Arrange
        SetupLines(_today, "11:11 test +feature .123", "11:20 test +feature .123");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities[0].Task.Title.Should().Be("test");
    }

    [Fact]
    public void WithOneEntry_WhenGetAllRecords_ThenReturnsOneRecord()
    {
        // Arrange
        SetupLines(_today, "11:12 This is a test +feature .123");
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime.AddMinutes(2));

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities[0].Records.Should().HaveCount(1);
        activities[0].Records.First().StartTime.Hour.Should().Be(11);
        activities[0].Records.First().StartTime.Minute.Should().Be(12);
        activities[0].Records.First().EndTime.Should().NotBeNull();
        activities[0].Records.First().EndTime?.Hour.Should().Be(11);
        activities[0].Records.First().EndTime?.Minute.Should().Be(14);
    }

    [Fact]
    public void WithTwoEntries_WhenGettingAllRecords_ThenFirstEntryEndsWhenSecondStarts()
    {
        // Arrange
        SetupLines(_today, "11:12 test1 +feature .123", "11:15 test2 +feature .456");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(2);
        activities.First().Records.First().EndTime.Should().NotBeNull();
        activities.First().Records.First().EndTime?.Hour.Should().Be(11);
        activities.First().Records.First().EndTime?.Minute.Should().Be(15);
    }

    [Fact]
    public void WithStoppedRecord_WhenGettingRecords_ThenRecordIsStoppedAtCorrectTime()
    {
        // Arrage
        SetupLines(_today, "11:11 test", "11:12 [Stop]");
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime.AddMinutes(5));

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities.First().Records.Should().HaveCount(1);
        activities.First().Records.First().EndTime.Should().NotBeNull();
        activities.First().Records.First().EndTime?.Hour.Should().Be(11);
        activities.First().Records.First().EndTime?.Minute.Should().Be(12);
    }

    [Fact]
    public void WithSameActivity_WithStoppedRecordInTheMiddle_ThenReturnsTwoRecordsWithCorrectEndTimes()
    {
        // Arrange
        SetupLines(_today, "11:10 test", "11:11 [Stop]", "11:15 test");
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime.AddMinutes(5));

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities.First().Records.Should().HaveCount(2);
        activities.First().Records.First().EndTime?.Hour.Should().Be(11);
        activities.First().Records.First().EndTime?.Minute.Should().Be(11);
        activities.First().Records.Skip(1).First().StartTime.Hour.Should().Be(11);
        activities.First().Records.Skip(1).First().StartTime.Minute.Should().Be(15);
        activities.First().Records.Skip(1).First().EndTime?.Hour.Should().Be(11);
        activities.First().Records.Skip(1).First().EndTime?.Minute.Should().Be(17);
    }

    [Fact]
    public void WithDotInTitle_WithNonNumericChars_WhenParsingActivityTitle_ThenReturnsTitleWithDot()
    {
        // Arrange
        SetupLines(_today, "11:11 test with .dot");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities[0].Task.Title.Should().Be("test with .dot");
    }

    [Fact]
    public void WithOneRecord_WithInvalidHour_WhenGettingRecords_ThenReturnsRecordStartingAtMidNight()
    {
        // Arrange
        SetupLines(_today, "xx:1x test with invalid hour");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities[0].Records.Should().HaveCount(1);
        activities[0].Records.First().StartTime.Hour.Should().Be(0);
        activities[0].Records.First().StartTime.Minute.Should().Be(0);
    }

    [Fact]
    public void WithOneRecord_WithoutDate_WhenParsingRecords_ThenReturnsOneRecrodWithDateTimeMinDate()
    {
        // Arrange
        SetupLines("11:00 test");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities[0].Records.Should().HaveCount(1);
        activities[0].Records.First().StartTime.Year.Should().Be(DateTime.MinValue.Year);
        activities[0].Records.First().StartTime.Month.Should().Be(DateTime.MinValue.Month);
        activities[0].Records.First().StartTime.Day.Should().Be(DateTime.MinValue.Day);
    }

    #endregion GetActivities

    private void SetupLines(params string[] lines)
    {
        var linesList = lines.Where(x => !string.IsNullOrEmpty(x)).ToList();
        _stream.Setup(x => x.ReadAllLines()).Returns(lines.ToList());
    }
}

