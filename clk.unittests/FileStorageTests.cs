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
    private readonly TaskLine _task;
    private readonly DateTime _baseTime = new DateTime(2022, 10, 10, 11, 12, 0);
    private const string _today = "[2022-10-10]";
    private const string _yesterday = "[2022-10-09]";

    public FileStorageTests()
    {
        _storage = _mocker.CreateInstance<FileStorage>();
        _stream = _mocker.GetMock<IStream>();
        _mocker.GetMock<ITimeProvider>().Setup(x => x.Now).Returns(_baseTime);
        _task = new TaskLine($"{_baseTime:HH:mm} Test #feature .123", _baseTime.Date);
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
        _storage.AddLine(_task);

        // Assert
        _stream.Verify(x => x.AddLine("[2022-10-10]"), shouldAddToday ? Times.Once : Times.Never);
        _stream.Verify(x => x.AddLine("11:12 Test #feature .123"), Times.Once);
    }

    [Fact]
    public void WithNonUniqueId_WhenAddingEntry_ThenThrowsException()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 .123");

        // Act
        var act = () => _storage.AddLine(new TaskLine("00:00 Activity2 .123"));

        // Assert
        act.Should().Throw<InvalidDataException>("Id 123 already exists");
    }

    [Fact]
    public void WithNonUniqueId_WhenAddingSameEntry_ThenDoesNotThrow()
    {
        // Arrange
        SetupLines(_today, "09:00 Activity1 .123");

        // Act
        var act = () => _storage.AddLine(new TaskLine("00:00 Activity1 .123"));

        // Assert
        act.Should().NotThrow<InvalidDataException>();
    }

    [Fact]
    public void WithMultipleLines_WhenReadingAll_ThenCorrectlyCalculatesTaskDurations()
    {
        // Arrange
        SetupLines(
            _yesterday,
            "09:00 Activity1 .001",
            "10:00 Activity2 .002",
            "11:00 Activity1 .001",
            "12:00 [Stop]",
            _today,
            "13:00 Activity2 .002",
            "15:30 [Stop]"
        );

        // Act
        var tasks = _storage.GetTasks().ToList();

        // Assert
        tasks.Count.Should().Be(6);
        tasks[0].EndTime?.Hour.Should().Be(10);
        tasks[0].Duration.Should().Be(TimeSpan.FromHours(1));
        tasks[2].Duration.Should().Be(TimeSpan.FromHours(1));
        tasks[4].Duration.Should().Be(TimeSpan.FromHours(2.5));
    }

    #endregion AddEntry

    private void SetupLines(params string[] lines)
    {
        var linesList = lines.Where(x => !string.IsNullOrEmpty(x)).ToList();
        _stream.Setup(x => x.ReadAllLines()).Returns(lines.ToList());
    }
}

