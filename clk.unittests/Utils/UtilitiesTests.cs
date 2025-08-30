using clk.Domain;
using clk.Utils;
using FluentAssertions;

namespace clk.unittests;

public class UtilitiesTests
{
    public UtilitiesTests()
    {
    }

    [Fact]
    public void WithMiddleOfWeek_WhenGettingMonday_ReturnsMonday()
    {
        // Act
        var day = new DateTime(2022, 10, 22).MondayOfTheWeek();

        // Assert
        day.Date.Day.Should().Be(17);
        day.Date.Month.Should().Be(10);
    }

    [Fact]
    public void WithMiddleOfWeek_WhenGettingFriday_ReturnsFriday()
    {
        // Act
        var day = new DateTime(2022, 10, 22).MondayOfTheWeek().AddDays(4);

        // Assert
        day.Date.Day.Should().Be(21);
        day.Date.Month.Should().Be(10);
    }

    [Fact]
    public void WithSingleDigitTime_WhenDisplay_ThenPadsWithZero()
    {
        // Arrange
        var records = Enumerable.Range(0, 1).Select(x => new Domain.Record(new DateTime(2022, 10, 10, 9, 8, 0), new DateTime(2022, 10, 10, 10, 9, 0)));

        // Act
        var result = records.Duration();

        // Assert
        result.Should().Be("01:01");
    }

    [Fact]
    public void WithSingleDigitTime_WhenDisplayActivity_ThenPadsWithZero()
    {
        // Arrange
        var records = Enumerable.Range(0, 1)
	        .Select(x => new Activity(
		        new Domain.Task("Test", [], [], ""), 
		        new List<Domain.Record>() { new Domain.Record(new DateTime(2022, 10, 10, 9, 8, 0), new DateTime(2022, 10, 10, 10, 9, 0)) }));

        // Act
        var result = records.Duration();

        // Assert
        result.Should().Be("01:01");
    }

    [Fact]
    public void WithMoreThanOneDayDuration_WhenDisplay_ThenDisplaysTotalHours()
    {
        // Arrange
        var records = Enumerable.Range(0, 1).Select(x => new Domain.Record(new DateTime(2022, 01, 01, 10, 0, 0), new DateTime(2022, 01, 02, 12, 0, 0)));

        // Act
        var result = records.Duration();

        // Assert
        result.Should().Be("26:00");
    }

    [Fact]
    public void WithMultipleNonOverlappingRecords_WhenGettingDuration_ThenComputesDurationCorrectly()
    {
        // Arrange
        var records = new List<Domain.Record>()
        {
            new Domain.Record(new DateTime(2022, 1, 1, 9, 0, 0), new DateTime(2022, 1, 1, 10, 0, 0)),
            new Domain.Record(new DateTime(2022, 1, 1, 11, 0, 0), new DateTime(2022, 1, 1, 12, 0, 0)),
        };

        // Act
        var result = records.Duration();

        // Assert
        result.Should().Be("02:00");
    }

    [Theory]
    [InlineData(1, 3, 1)]
    [InlineData(5, 9, 19)]
    [InlineData(12, 31, 52)]
    public void WhenGetWeekNumber_ReturnsCorrectWeekNumber(int month, int day, int expectedWeekNumber)
    {
        Utilities.GetWeekNumber(new DateTime(2022, month, day)).Should().Be(expectedWeekNumber);
    }
}

