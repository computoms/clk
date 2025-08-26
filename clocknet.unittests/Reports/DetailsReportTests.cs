using clocknet.Domain;
using clocknet.Domain.Reports;
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
                new Domain.Task("Activity1", new string[] { }, "001"),
                new List<Domain.Record>()
                {
                    new Domain.Record(Date(9), Date(10)),
                    new Domain.Record(Date(11), Date(11, 30)),
                }),
            new Activity(
                new Domain.Task("Activity2", new string[] { "tag2", "tag3" }, "002"),
                new List<Domain.Record>()
                {
                    new Domain.Record(Date(13), Date(13, 8)),
                    new Domain.Record(Date(14), Date(14, 27)),
                }),
        };
        var expectedOutput = new List<string>()
        {
            "2022-01-01",
            "  01:30 Activity1 .001",
            "     01:00 (09:00 -> 10:00)",
            "     00:30 (11:00 -> 11:30)",
            "  00:35 Activity2 +tag2 +tag3 .002",
            "     00:08 (13:00 -> 13:08)",
            "     00:27 (14:00 -> 14:27)",
            " ",
            "02:05 Total",
            " ",
            " --> 00:35 Activity2 .002"
        };
        var calledStrings = new List<FormattedLine>();
        _display.Setup(x => x.Print(It.IsAny<IEnumerable<FormattedLine>>()))
            .Callback((IEnumerable<FormattedLine> strings) => calledStrings = strings.ToList());
        _display.Setup(x => x.Layout(It.IsAny<IEnumerable<FormattedText>>(), It.IsAny<int>()))
            .Returns((IEnumerable<FormattedText> str, int tabs) =>
                new FormattedLine
                {
                    Chunks = str.Prepend(new()
                    {
                        RawText = string.Join(' ', Enumerable.Range(0, tabs).Select(x => "  ")),
                        Color = Console.ForegroundColor
                    })
                });


        // Act
        _report.Print(activities);

        // Assert
        _display.Verify(x => x.Print(It.IsAny<IEnumerable<FormattedLine>>()));
        calledStrings.Count.Should().Be(expectedOutput.Count);
        calledStrings.First().Chunks.First().RawText.Should().Be(expectedOutput.First());
        calledStrings.Skip(1).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(1).First());
        calledStrings.Skip(2).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(2).First());
        calledStrings.Skip(3).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(3).First());
        calledStrings.Skip(4).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(4).First());
        calledStrings.Skip(5).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(5).First());
        calledStrings.Skip(6).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(6).First());
        calledStrings.Skip(7).First().Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(expectedOutput.Skip(7).First());

    }

    private DateTime Date(int hours, int minutes = 0) => new DateTime(2022, 1, 1, hours, minutes, 0);
}

