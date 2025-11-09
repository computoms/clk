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

    [Fact]
    public void WithTwoTasks_WithStopTask_WhenGetAll_ThenReturnsOnlyNonStopTasks()
    {
        // Arrange
        var task1 = new TaskLine("09:00 Task1 .123");
        var task2 = new TaskLine("10:00 Task2 .124");
        var stopTask = new TaskLine("11:00 [Stop]");
        _storage.Setup(x => x.GetTasks()).Returns(new List<TaskLine>() { task1, stopTask, task2 });

        // Act
        var result = _repository.GetAll();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(task1);
        result.Should().Contain(task2);
        result.Should().NotContain(stopTask);
    }

    [Fact]
    public void WhenAddingTask_ThenTaskIsAddedToStorage()
    {
        // Arrange
        var task = new TaskLine("09:00 Task1 .123");

        // Act
        _repository.AddTask(task);

        // Assert
        _storage.Verify(x => x.AddLine(task), Times.Once);
    }

    [Fact]
    public void WithThrowingStorage_WhenAddingEntry_ThenDoesNotThrow()
    {
        // Arrange
        _storage.Setup(x => x.AddLine(It.IsAny<TaskLine>())).Throws<Exception>();

        // Act
        var act = () => _repository.AddTask(new TaskLine("09:00 Task1 .123"));

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void WithOneTaskWithOneTag_WhenFilterByTag_ThenReturnsEntry()
    {
        // Arrange
        _storage.Setup(x => x.GetTasks()).Returns(new List<TaskLine>() { new TaskLine("10:00 Entry #tag .123"), new TaskLine("11:00 Entry .123") });

        // Act
        var result = _repository.FilterByQuery(new RepositoryQuery(null, null, null, new List<string>{"tag"}, null));

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Entry");
        result.First().Id.Should().Be("123");
    }

    [Fact]
    public void WithTwoEntries_WithTwoTags_WhenFilterByTag_ThenReturnsCorrectEntry()
    {
        // Arrange
        _storage.Setup(x => x.GetTasks()).Returns(new List<TaskLine>()
        {
            new TaskLine("10:00 Entry1 #tag1 #tag2 .123"),
            new TaskLine("11:00 Entry2 #tag1 #tag3 .345"),
        });

        // Act
        var result = _repository.FilterByQuery(new RepositoryQuery(null, null, null, new List<string> { "tag1", "tag2" }, null));

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Entry1");
        result.First().Id.Should().Be("123");
    }

    [Fact]
    public void WhenFilterByDate_ThenReturnsCorrectTasks()
    {
        // Arrange
        _storage.Setup(x => x.GetTasks())
            .Returns(new List<TaskLine>()
            {
                new TaskLine("10:00 Entry1 #tag1 .123", new DateTime(2022, 10, 10)),
                new TaskLine("11:00 Entry2 #tag2 .345", new DateTime(2022, 10, 11)),
                new TaskLine("12:00 Entry3 #tag3 .567", new DateTime(2022, 10, 12)),
            });

        // Act
        var result = _repository.FilterByQuery(new RepositoryQuery(new DateTime(2022, 10, 11), new DateTime(2022, 10, 12), null, null, null));

        // Assert
        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Entry2");
        result.Last().Title.Should().Be("Entry3");
    }

    [Fact]
    public void WhenFilterByPath_ThenReturnsCorrectTasks()
    {
        // Arrange
        _storage.Setup(x => x.GetTasks())
            .Returns(new List<TaskLine>()
            {
                new TaskLine("10:00 Entry1 #tag1 /project/task .123"),
                new TaskLine("11:00 Entry2 #tag2 /project .345"),
                new TaskLine("12:00 Entry3 #tag3 /otherproject/task .567"),
            });

        // Act
        var result = _repository.FilterByQuery(new RepositoryQuery(null, null, new List<string> { "project", "task" }, null, null));

        // Assert
        result.Should().HaveCount(1);
        result.First().Title.Should().Be("Entry1");
    }
}
