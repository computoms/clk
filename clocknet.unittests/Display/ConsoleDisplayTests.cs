using clocknet.Display;
using FluentAssertions;

namespace clocknet.unittests;

public class ConsoleDisplayTests
{
    private readonly ConsoleDisplay _display = new();

    public ConsoleDisplayTests()
    {
    }

    [Fact]
    public void WithNoTabs_WhenLayout_ThenReturnsStringAsIs()
    {
        // Act
        var result = _display.Layout("test");

        // Assert
        result.Should().Be("test");
    }

    [Fact]
    public void WithOneTab_WhenLayout_ThenAddsTwoSpaces()
    {
        // Act
        var result = _display.Layout("test", 1);

        // Assert
        result.Should().Be("  test");
    }
}

