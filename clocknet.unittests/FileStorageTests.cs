using clocknet.Storage;
using Moq;
using Moq.AutoMock;
using FluentAssertions;

namespace clocknet.unittests;

public class FileStorageTests
{
    private readonly AutoMocker _mocker = new();
    private readonly FileStorage _storage;
    private readonly Mock<IStream> _stream;
    private readonly Activity _activity;
    private readonly Record _record;
    private readonly DateTime _baseTime = new DateTime(2022, 10, 10, 11, 12, 0);
    private const string _today = "[2022-10-10]";
    private const string _yesterday = "[2022-10-09]";

    public FileStorageTests()
    {
        _storage = _mocker.CreateInstance<FileStorage>();
        _stream = _mocker.GetMock<IStream>();
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime);
        _activity = new Activity(0, "Test", new string[] { "feature" }, "123");
        _record = new Record(0, _baseTime, null);
    }

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
        activities[0].Title.Should().Be("This is a test");
        activities[0].Tags.Should().HaveCount(1);
        activities[0].Tags[0].Should().Be("feature");
        activities[0].Number.Should().Be("123");
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
        activities[0].Title.Should().Be("test");
    }

    [Fact]
    public void WithOneEntry_WhenGetAllRecords_ThenReturnsOneRecord()
    {
        // Arrange
        SetupLines(_today, "11:12 This is a test +feature .123");
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime.AddMinutes(2));

        // Act
        var records = _storage.GetRecords();

        // Assert
        records.Should().HaveCount(1);
        records[0].ActivityId.Should().Be(1);
        records[0].StartTime.Hour.Should().Be(11);
        records[0].StartTime.Minute.Should().Be(12);
        records[0].EndTime.Should().NotBeNull();
        records[0].EndTime?.Hour.Should().Be(11);
        records[0].EndTime?.Minute.Should().Be(14);
    }

    [Fact]
    public void WithTwoEntries_WhenGettingAllRecords_ThenFirstEntryEndsWhenSecondStarts()
    {
        // Arrange
        SetupLines(_today, "11:12 test1 +feature .123", "11:15 test2 +feature .456");

        // Act
        var records = _storage.GetRecords();

        // Assert
        records.Should().HaveCount(2);
        records[0].EndTime.Should().NotBeNull();
        records[0].EndTime?.Hour.Should().Be(11);
        records[0].EndTime?.Minute.Should().Be(15);
    }

    [Fact]
    public void WithStoppedRecord_WhenGettingRecords_ThenRecordIsStoppedAtCorrectTime()
    {
        // Arrage
        SetupLines(_today, "11:11 test", "11:12 [Stop]");
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime.AddMinutes(5));

        // Act
        var records = _storage.GetRecords();

        // Assert
        records.Should().HaveCount(1);
        records[0].EndTime.Should().NotBeNull();
        records[0].EndTime?.Hour.Should().Be(11);
        records[0].EndTime?.Minute.Should().Be(12);
    }

    [Fact]
    public void WithSameActivity_WithStoppedRecordInTheMiddle_ThenReturnsTwoRecordsWithCorrectEndTimes()
    {
        // Arrange
        SetupLines(_today, "11:10 test", "11:11 [Stop]", "11:15 test");
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime.AddMinutes(5));

        // Act
        var records = _storage.GetRecords();

        // Assert
        records.Should().HaveCount(2);
        records[0].EndTime?.Hour.Should().Be(11);
        records[0].EndTime?.Minute.Should().Be(11);
        records[1].StartTime.Hour.Should().Be(11);
        records[1].StartTime.Minute.Should().Be(15);
        records[1].EndTime?.Hour.Should().Be(11);
        records[1].EndTime?.Minute.Should().Be(17);
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
        activities[0].Title.Should().Be("test with .dot");
    }

    [Fact]
    public void WithOneRecord_WithInvalidHour_WhenGettingRecords_ThenReturnsRecordStartingAtMidNight()
    {
        // Arrange
        SetupLines(_today, "xx:1x test with invalid hour");

        // Act
        var records = _storage.GetRecords();

        // Assert
        records.Should().HaveCount(1);
        records[0].StartTime.Hour.Should().Be(0);
        records[0].StartTime.Minute.Should().Be(0);
    }

    [Fact]
    public void WithOneRecord_WithoutDate_WhenParsingRecords_ThenReturnsOneRecrodWithDateTimeMinDate()
    {
        // Arrange
        SetupLines("11:00 test");

        // Act
        var records = _storage.GetRecords();

        // Assert
        records.Should().HaveCount(1);
        records[0].StartTime.Year.Should().Be(DateTime.MinValue.Year);
        records[0].StartTime.Month.Should().Be(DateTime.MinValue.Month);
        records[0].StartTime.Day.Should().Be(DateTime.MinValue.Day);
    }

    private void SetupLines(params string[] lines)
    {
        var linesList = lines.ToList();
        _stream.Setup(x => x.ReadAllLines()).Returns(lines.ToList());
    }
}

