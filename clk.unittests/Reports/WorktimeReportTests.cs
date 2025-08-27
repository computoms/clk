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
        var activities = new List<Activity>()
        {
            // Day 1: 5:30
            new Activity(
		        new Domain.Task("Activity1", new string[] { "tag" }, "001"),
                new List<Domain.Record>()
                {
                    new Domain.Record(Date(9), Date(10)),
                    new Domain.Record(Date(11), Date(12)),
		        }),
            new Activity(
		        new Domain.Task("Activity2", new string[] { "tag" }, "002"),
                new List<Domain.Record>()
                {
                    new Domain.Record(Date(10), Date(11)),
                    new Domain.Record(Date(13), Date(15, 30)),
		        }),
            // Day2: 5:32
            new Activity(
		        new Domain.Task("Activity1", new string[] { "tag" }, "001"),
                new List<Domain.Record>()
                {
                    new Domain.Record(Date(10, 0, 2), Date(11, 0, 2)),
                    new Domain.Record(Date(13, 0, 2), Date(15, 30, 2)),
		        }),
            new Activity(
		        new Domain.Task("Activity2", new string[] { "tag" }, "002"),
                new List<Domain.Record>()
                {
                    new Domain.Record(Date(11, 0, 2), Date(12, 0, 2)),
                    new Domain.Record(Date(15, 30, 2), Date(16, 32, 2)),
		        }),
        };
        var expectedOutputPerDay = new List<string>()
        {
            "Daily Worktime Report",
            "05:30 2022-01-01",
            "05:32 2022-01-02",
            " ",
            "11:02 Total",
        };
        var expectedOutputTotal = new List<string>()
        {
            "Weekly Report",
            "11:02 Week 52",
            " ",
            "11:02 Total",
        };
        var calledStrings = new List<FormattedLine>();
        _display.Setup(x => x.Print(It.IsAny<IEnumerable<FormattedLine>>()))
            .Callback((IEnumerable<FormattedLine> strings) => calledStrings = strings.ToList());
        _display.Setup(x => x.Layout(It.IsAny<IEnumerable<FormattedText>>(), It.IsAny<int>()))
            .Returns((string str, int tabs) => (string.Join(' ', Enumerable.Range(0, tabs).Select(x => "  ")) + str).FormatLine());

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

