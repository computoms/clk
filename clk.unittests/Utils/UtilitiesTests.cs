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

    [Theory]
    [InlineData(1, 3, 1)]
    [InlineData(5, 9, 19)]
    [InlineData(12, 31, 52)]
    public void WhenGetWeekNumber_ReturnsCorrectWeekNumber(int month, int day, int expectedWeekNumber)
    {
        Utilities.GetWeekNumber(new DateTime(2022, month, day)).Should().Be(expectedWeekNumber);
    }
}

