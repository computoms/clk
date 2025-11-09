namespace clk.unittests;

using System.Runtime.InteropServices;
using clk.Domain;
using clk.Infra;
using clk.Utils;
using FluentAssertions;
using Moq;

public class FilterParserTests
{
    private Mock<ITimeProvider> timeProvider = new Mock<ITimeProvider>();
    private Mock<IStorage> storage = new Mock<IStorage>();
    private readonly RecordRepository repository;
    private FilterParser filterParser;

    public FilterParserTests()
    {
        repository = new RecordRepository(storage.Object, timeProvider.Object);

        storage.Setup(x => x.GetTasks()).Returns(new List<TaskLine>()
        {
            new TaskLine("09:00 Task1 #tag1 .123", new DateTime(2022, 10, 10)) { EndTime = new DateTime(2022, 10, 10, 10, 0, 0) },
            new TaskLine("10:00 [Stop]", new DateTime(2022, 10, 10)),
            new TaskLine("10:00 Task2 /path1/path2 #tag2 .124", new DateTime(2022, 10, 11)) { EndTime = new DateTime(2022, 10, 11, 11, 0, 0) },
            new TaskLine("11:00 [Stop]", new DateTime(2022, 10, 11)),
            new TaskLine("11:00 Task3 /path1 #tag1 #tag2 .125", new DateTime(2022, 10, 12)) { EndTime = new DateTime(2022, 10, 12, 12, 0, 0) },
            new TaskLine("12:00 [Stop]", new DateTime(2022, 10, 12)),
        });

        timeProvider.Setup(x => x.Now).Returns(new DateTime(2022, 10, 12, 13, 0, 0));
    }

    [Theory]
    [InlineData(new string[2] { "show", "--all" }, 3)]
    [InlineData(new string[2] { "show", "--yesterday" }, 1)]
    [InlineData(new string[2] { "show", "--week" }, 3)]
    [InlineData(new string[1] { "show" }, 1)]
    [InlineData(new string[4] { "show", "--all", "--last", "2" }, 2)]
    [InlineData(new string[4] { "show", "--all", "--tags", "tag2" }, 2)]
    [InlineData(new string[4] { "show", "--all", "--path", "/path1" }, 2)]
    public void WithArguments_WhenFilter_ThenExecutesQuery(string[] args, int expectedCount)
    {
        // Arrange
        var pArgs = new ProgramArguments(args);
        filterParser = new FilterParser(pArgs, repository, timeProvider.Object);

        // Act
        var results = filterParser.Filter();

        // Assert
        results.Should().HaveCount(expectedCount);
    }
}