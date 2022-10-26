using clocknet.Storage;
using Moq;
using Moq.AutoMock;
using FluentAssertions;
using clocknet.Utils;

namespace clocknet.unittests;

public class FileStorageTests
{
    private readonly AutoMocker _mocker = new();
    private readonly FileStorage _storage;
    private readonly Mock<IStream> _stream;
    private readonly Task _activity;
    private readonly Record _record;
    private readonly DateTime _baseTime = new DateTime(2022, 10, 10, 11, 12, 0);
    private const string _today = "[2022-10-10]";
    private const string _yesterday = "[2022-10-09]";

    public FileStorageTests()
    {
        _storage = _mocker.CreateInstance<FileStorage>();
        _stream = _mocker.GetMock<IStream>();
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime);
        _activity = new Task("Test", new string[] { "feature" }, "123");
        _record = new Record(_baseTime, null);
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

    [Fact]
    public void WithMultipleTags_WhenAddingEntry_ThenAddsMultipleTags()
    {
        // Arrange
        SetupLines(_today);

        // Act
        _storage.AddEntry(_activity with { Tags = new string[] { "feature", "test" } }, _record);

        // Assert
        _stream.Verify(x => x.AddLine("11:12 Test +feature +test .123"), Times.Once);
    }

    [Fact]
    public void WithNoTags_WhenAddingEntry_ThenAddsEntryWithoutExtraSpaces()
    {
        // Arrange
        SetupLines(_today);

        // Act
        _storage.AddEntry(_activity with { Tags = new string[0] }, _record);

        // Assert
        _stream.Verify(x => x.AddLine("11:12 Test .123"), Times.Once);
    }

    [Fact]
    public void WithNoId_WhenAddingEntry_ThenAddsEntryWithoutExtraSpaces()
    {
        // Arrange
        SetupLines(_today);

        // Act
        _storage.AddEntry(_activity with { Id = "" }, _record);

        // Assert
        _stream.Verify(x => x.AddLine("11:12 Test +feature"), Times.Once);
    }

    [Fact]
    public void WithNonUniqueId_WhenAddingEntry_ThenThrowsException()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 .123");

        // Act
        var act = () => _storage.AddEntry(new Task("Activity2", new string[0], "123"), new Record(DateTime.Now, null));

        // Assert
        act.Should().Throw<InvalidDataException>("Id 123 already exists");
    }

    [Fact]
    public void WithNonUniqueId_WhenAddingSameEntry_ThenDoesNotThrow()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 .123");

        // Act
        var act = () => _storage.AddEntry(new Task("Activity1", new string[0], "123"), new Record(DateTime.Now, null));

        // Assert
        act.Should().NotThrow<InvalidDataException>();
    }

    #endregion AddEntry

    #region AddRawEntry

    [Fact]
    public void WithStandardEntry_WhenAddingRawEntry_ThenAddsEntry()
    {
        // Arrange
        SetupLines(_today);

        // Act
        _storage.AddEntryRaw("This is a new entry +tag1 +tag2 .123");

        // Assert
        _stream.Verify(x => x.AddLine("11:12 This is a new entry +tag1 +tag2 .123"));
    }

    [Fact]
    public void WithOnlyId_WhenAddingRawEntry_ThenAddsCorrespondingActivityEntry()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 +tag .123");

        // Act
        _storage.AddEntryRaw(".123");

        // Assert
        _stream.Verify(x => x.AddLine("11:12 Activity1 +tag .123"));
    }

    [Fact]
    public void WithEmptyId_WhenAddingRawEntry_ThenAddsNewEntry()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 +tag .123");

        // Act
        _storage.AddEntryRaw("Test .");

        // Assert
        _stream.Verify(x => x.AddLine("11:12 Test"));
    }

    [Fact]
    public void WithNonMatchingTitleWithId_WhenAddingRawEntry_ThenThrowsException()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 +tag .123");

        // Act
        var act = () => _storage.AddEntryRaw("Activity2 .123");

        // Assert
        act.Should().Throw<InvalidDataException>("Id 123 already exists");
    }

    #endregion AddRawEntry

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

    [Fact]
    public void WithOneEntry_WhenGetAllActivities_ThenReturnsOneActivity()
    {
        // Arrange
        SetupLines(_today, "11:11 This is a test +feature .123");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities.Should().HaveCount(1);
        activities[0].Task.Title.Should().Be("This is a test");
        activities[0].Task.Tags.Should().HaveCount(1);
        activities[0].Task.Tags[0].Should().Be("feature");
        activities[0].Task.Id.Should().Be("123");
    }

    [Fact]
    public void WithWhiteSpacesBeforeAfter_WhenGetActivites_ThenTrimsWhiteSpaces()
    {
        // Arrange
        SetupLines(_today, "   11:11 Test activity +feature .123   ");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities[0].Task.Title.Should().Be("Test activity");
    }

    [Fact]
    public void WithOnlyId_WhenGetActivities_ThenFormatsCorrectly()
    {
        // Arrange
        SetupLines(_today, "11:11 Test activity .123");

        // Act
        var activities = _storage.GetActivities();

        // Assert
        activities[0].Task.Title.Should().Be("Test activity");
        activities[0].Task.Id.Should().Be("123");
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
        var linesList = lines.ToList();
        _stream.Setup(x => x.ReadAllLines()).Returns(lines.ToList());
    }
}

