using clk.Domain;
using clk.Infra;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace clk.unittests;

public class RecordRepositoryTests
{
    private readonly AutoMocker _mocker = new();
    private readonly RecordRepository _repository;
    private readonly Mock<IStorage> _storage;

    public RecordRepositoryTests()
    {
        _repository = _mocker.CreateInstance<RecordRepository>();
        _storage = _mocker.GetMock<IStorage>();
    }

    [Theory]
    [InlineData("New Entry", "123")]
    [InlineData(" ", "123")]
    public void WhenAddingRawEntry_ThenEntryIsAddedToStorage(string title, string id)
    {
        // Arrange
        var task = new Domain.Task(title, [], [], id);
        var record = new Domain.Record(DateTime.Now);

        // Act
        _repository.AddRecord(task, record);

        // Assert
        _storage.Verify(x => x.AddEntry(task, record), Times.Once);
    }

    [Fact]
    public void WithThrowingStorage_WhenAddingEntry_ThenDoesNotThrow()
    {
        // Arrange
        _storage.Setup(x => x.AddEntry(It.IsAny<Domain.Task>(), It.IsAny<Domain.Record>())).Throws<InvalidDataException>();

        // Act
        var act = () => _repository.AddRecord(new Domain.Task("Test", [], [], ""), new Domain.Record(DateTime.Now, null));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WhenAddingEntry_ThenEntryIsAddedToStorage()
    {
        // Arrange
        var activity = new Domain.Task("Test", [], [], "");
        var record = new Domain.Record(DateTime.Now, null);

        // Act
        _repository.AddRecord(activity, record);

        // Assert
        _storage.Verify(x => x.AddEntry(activity, record), Times.Once);
    }

    [Fact]
    public void WithOneEntryWithOneTag_WhenFilterByTag_ThenReturnsEntry()
    {
        // Arrange
        _storage.Setup(x => x.GetActivities()).Returns(new List<Activity>() { new Activity(new Domain.Task("Entry", [], ["tag"], "123")) });

        // Act
        var result = _repository.FilterByTag(new List<string>() { "tag" });

        // Assert
        result.Should().HaveCount(1);
        result.First().Task.Title.Should().Be("Entry");
        result.First().Task.Id.Should().Be("123");
    }

    [Fact]
    public void WithOneEntryWithTwoTags_WhenFilterByTag_ThenReturnsEntry()
    {
        // Arrange
        _storage.Setup(x => x.GetActivities()).Returns(new List<Activity>() { new Activity(new Domain.Task("Entry", [], ["tag1", "tag2"], "123")) });

        // Act
        var result = _repository.FilterByTag(new List<string>() { "tag1" });

        // Assert
        result.Should().HaveCount(1);
        result.First().Task.Title.Should().Be("Entry");
        result.First().Task.Id.Should().Be("123");
    }

    [Fact]
    public void WithTwoEntries_WithTwoTags_WhenFilterByTag_ThenReturnsCorrectEntry()
    {
        // Arrange
        _storage.Setup(x => x.GetActivities()).Returns(
	    new List<Activity>() 
	    { 
	        new Activity(new Domain.Task("Entry1", [], ["tag1", "tag2"], "123")),
	        new Activity(new Domain.Task("Entry2", [], ["tag1", "tag3"], "345")),
	    });

        // Act
        var result = _repository.FilterByTag(new List<string>() { "tag1", "tag2" });

        // Assert
        result.Should().HaveCount(1);
        result.First().Task.Title.Should().Be("Entry1");
        result.First().Task.Id.Should().Be("123");
    }

    [Fact]
    public void WithOneActivity_WithTwoRecords_WhenFilterByDate_ThenReturnsCorrectRecords()
    {
        // Arrange
        var activity = new Activity(new Domain.Task("Entry1", [], [], ""));
        var startTime = new DateTime(2022, 10, 10, 10, 0, 0);
        activity.AddRecord(new Domain.Record(startTime, new DateTime(2022, 10, 10, 11, 0, 0)));
        activity.AddRecord(new Domain.Record(new DateTime(2022, 10, 11, 10, 0, 0), new DateTime(2022, 10, 11, 11, 0, 0)));
        _storage.Setup(x => x.GetActivities())
            .Returns(
                new List<Activity>()
                {
                    activity
                });

        // Act
        var result = _repository.FilterByDate(new DateTime(2022, 10, 10));

        // Assert
        result.First().Records.Should().HaveCount(1);
        result.First().Records.First().StartTime.Should().Be(startTime);
    }

    [Fact]
    public void WithTwoActivities_WhenFilter_ThenActivityNotConainingRecordsAreNotReturned()
    {
        // Arrange
        _storage.Setup(x => x.GetActivities())
            .Returns(new List<Activity>()
            {
                new Activity(new Domain.Task("Activtiy1", [], [], ""), new List<Domain.Record>(){new Domain.Record(new DateTime(2022, 10, 10), new DateTime(2022, 10, 10))}),
                new Activity(new Domain.Task("Activtiy2", [], [], ""), new List<Domain.Record>(){new Domain.Record(new DateTime(2022, 10, 11), new DateTime(2022, 10, 11))}),
            });

        // Act
        var result = _repository.FilterByDate(new DateTime(2022, 10, 10));

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public void DisplayDuration()
    {
        TimeSpan ts = new TimeSpan(10, 11, 0);
        var str = ts.ToString(@"hh\:mm");
        str.Should().Be("10:11");
    }
}
