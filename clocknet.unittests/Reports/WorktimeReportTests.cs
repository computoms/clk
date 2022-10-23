using clocknet.Display;
using clocknet.Reports;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace clocknet.unittests.Reports;

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
		        new Task("Activity1", new string[] { "tag" }, "001"),
                new List<Record>()
                {
                    new Record(Date(9), Date(10)),
                    new Record(Date(11), Date(12)),
		        }),
            new Activity(
		        new Task("Activity2", new string[] { "tag" }, "002"),
                new List<Record>()
                {
                    new Record(Date(10), Date(11)),
                    new Record(Date(13), Date(15, 30)),
		        }),
            // Day2: 5:32
            new Activity(
		        new Task("Activity1", new string[] { "tag" }, "001"),
                new List<Record>()
                {
                    new Record(Date(10, 0, 2), Date(11, 0, 2)),
                    new Record(Date(13, 0, 2), Date(15, 30, 2)),
		        }),
            new Activity(
		        new Task("Activity2", new string[] { "tag" }, "002"),
                new List<Record>()
                {
                    new Record(Date(11, 0, 2), Date(12, 0, 2)),
                    new Record(Date(15, 30, 2), Date(16, 32, 2)),
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
        var calledStrings = new List<string>();
        _display.Setup(x => x.Print(It.IsAny<IEnumerable<string>>()))
            .Callback((IEnumerable<string> strings) => calledStrings = strings.ToList());
        _display.Setup(x => x.Layout(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string str, int tabs) => string.Join(' ', Enumerable.Range(0, tabs).Select(x => "  ")) + str);

        // Act
        report.Print(activities);

        // Assert
        _display.Verify(x => x.Print(It.IsAny<IEnumerable<string>>()));
        if (perDay)
            calledStrings.Should().BeEquivalentTo(expectedOutputPerDay);
        else
            calledStrings.Should().BeEquivalentTo(expectedOutputTotal);
    }

    private DateTime Date(int hours, int minutes = 0, int day = 1) => new DateTime(2022, 1, day, hours, minutes, 0);
}

