using FluentAssertions;

namespace clocknet.unittests;

public class UtilitiesTests
{
    public UtilitiesTests()
    {
    }

    [Fact]
    public void WithMiddleOfWeek_WhenGettingMonday_ReturnsMonday()
    {
        // Act
        var day = new DateTime(2022, 10, 22).DayOfSameWeek(DayOfWeek.Monday);

        // Assert
        day.Date.Day.Should().Be(17);
        day.Date.Month.Should().Be(10);
    }

    [Fact]
    public void WithMiddleOfWeek_WhenGettingFriday_ReturnsFriday()
    {
        // Act
        var day = new DateTime(2022, 10, 22).DayOfSameWeek(DayOfWeek.Friday);

        // Assert
        day.Date.Day.Should().Be(21);
        day.Date.Month.Should().Be(10);
    }
}

