using clk.Domain;
using clk.Domain.Reports;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace clk.unittests.Reports;

public class WorktimeReportTests
{
    private readonly Mock<IDisplay> _display = new();
    private readonly AutoMocker _mocker = new();

    public WorktimeReportTests()
    {
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WithSomeActivites_WhenPrintingPerDay_ThenPrintsWorkTimes(bool perDay)
    {
        // Arrange
        var report = new WorktimeReport(_display.Object, perDay);
        var activities = new List<TaskLine>()
        {
            // Day 1: 5:30
            new TaskLine("09:00 Activity1 #tag .001", new DateTime(2022, 1, 1)) { EndTime = Date(10, 0, 1) },
            new TaskLine("10:00 Activity2 #tag .002", new DateTime(2022, 1, 1)) { EndTime = Date(11, 0, 1) },
            new TaskLine("11:00 Activity1 #tag .001", new DateTime(2022, 1, 1)) { EndTime = Date(12, 0, 1) },
            new TaskLine("13:00 Activity2 #tag .002", new DateTime(2022, 1, 1)) { EndTime = Date(15, 30, 1) },
            // Day2: 4:02
            new TaskLine("10:00 Activity1 #tag .001", new DateTime(2022, 1, 2)) { EndTime = Date(11, 0, 2) },
            new TaskLine("11:00 Activity2 #tag .002", new DateTime(2022, 1, 2)) { EndTime = Date(12, 30, 2) },
            new TaskLine("13:00 Activity1 #tag .001", new DateTime(2022, 1, 2)) { EndTime = Date(14, 0, 2) },
            new TaskLine("15:30 Activity2 #tag .002", new DateTime(2022, 1, 2)) { EndTime = Date(16, 2, 2) },
        };
        var expectedOutputPerDay = new List<string>()
        {
            "Daily Worktime Report",
            "05:30 2022-01-01",
            "04:02 2022-01-02",
            " ",
            "09:32 Total",
        };
        var expectedOutputTotal = new List<string>()
        {
            "Weekly Report",
            "09:32 Week 52",
            " ",
            "09:32 Total",
        };
        var calledStrings = new List<FormattedLine>();
        _display.Setup(x => x.Print(It.IsAny<IEnumerable<FormattedLine>>()))
            .Callback((IEnumerable<FormattedLine> strings) => calledStrings = strings.ToList());
        _display.Setup(x => x.Layout(It.IsAny<IEnumerable<FormattedText>>(), It.IsAny<int>()))
            .Returns((string str, int tabs) => (string.Join(' ', Enumerable.Range(0, tabs).Select(x => "  ")) + str).AsLine());

        // Act
        report.Print(activities);

        // Assert
        _display.Verify(x => x.Print(It.IsAny<IEnumerable<FormattedLine>>()));
        if (perDay)
        {
            calledStrings.Count.Should().Be(expectedOutputPerDay.Count);
            calledStrings.First().Chunks.First().RawText.Should().Be(expectedOutputPerDay.First());
            calledStrings.Skip(1).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputPerDay.Skip(1).First());
            calledStrings.Skip(2).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputPerDay.Skip(2).First());
            calledStrings.Skip(3).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputPerDay.Skip(3).First());
            calledStrings.Skip(4).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputPerDay.Skip(4).First());
        }
        else
        {
            calledStrings.Count.Should().Be(expectedOutputTotal.Count);
            calledStrings.First().Chunks.First().RawText.Should().Be(expectedOutputTotal.First());
            calledStrings.Skip(1).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputTotal.Skip(1).First());
            calledStrings.Skip(2).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputTotal.Skip(2).First());
            calledStrings.Skip(3).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutputTotal.Skip(3).First());
        }
    }

    private DateTime Date(int hours, int minutes = 0, int day = 1) => new DateTime(2022, 1, day, hours, minutes, 0);
}

