using clocknet.Display;
using clocknet.Reports;
using FluentAssertions;
using Moq;
using Moq.AutoMock;

namespace clocknet.unittests.Reports;

public class DetailsReportTests
{
    private readonly DetailsReport _report;
    private AutoMocker _mocker = new();
    private Mock<IDisplay> _display;

    public DetailsReportTests()
    {
        _report = _mocker.CreateInstance<DetailsReport>();
        _display = _mocker.GetMock<IDisplay>();
    }

    [Fact]
    public void WithSomeActivities_WhenPrinting_ThenPrintsDetails()
    {
        // Arrange
        var activities = new List<Activity>()
        {
            new Activity(
                new Task("Activity1", new string[] { "tag1" }, "001"),
                new List<Record>()
                {
                    new Record(Date(9), Date(10)),
                    new Record(Date(11), Date(11, 30)),
                }),
            new Activity(
                new Task("Activity2", new string[] { "tag2", "tag3" }, "002"),
                new List<Record>()
                {
                    new Record(Date(13), Date(13, 8)),
                    new Record(Date(14), Date(14, 27)),
                }),
        };
        var expectedOutput = new List<string>()
        {
            "01:30 Activity1 +tag1 .001",
            "     01:00 (09:00 -> 10:00)",
            "     00:30 (11:00 -> 11:30)",
            "00:35 Activity2 +tag2 +tag3 .002",
            "     00:08 (13:00 -> 13:08)",
            "     00:27 (14:00 -> 14:27)",
            " ",
            "02:05 Total",
        };
        var calledStrings = new List<string>();
        _display.Setup(x => x.Print(It.IsAny<IEnumerable<string>>()))
            .Callback((IEnumerable<string> strings) => calledStrings = strings.ToList());
        _display.Setup(x => x.Layout(It.IsAny<string>(), It.IsAny<int>()))
            .Returns((string str, int tabs) => string.Join(' ', Enumerable.Range(0, tabs).Select(x => "  ")) + str);

        // Act
        _report.Print(activities);

        // Assert
        _display.Verify(x => x.Print(It.IsAny<IEnumerable<string>>()));
        calledStrings.Should().BeEquivalentTo(expectedOutput);
    }

    private DateTime Date(int hours, int minutes = 0) => new DateTime(2022, 1, 1, hours, minutes, 0);
}

