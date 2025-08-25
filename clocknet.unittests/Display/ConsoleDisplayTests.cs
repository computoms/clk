using clocknet.Domain;
using clocknet.Infra;
using FluentAssertions;

namespace clocknet.unittests;

public class ConsoleDisplayTests
{
    private readonly ConsoleDisplay _display = new(true);

    public ConsoleDisplayTests()
    {
    }

    [Fact]
    public void WithNoTabs_WhenLayout_ThenReturnsStringAsIs()
    {
        // Act
        var result = _display.Layout(new List<FormattedText>() { "test".FormatChunk() });

        // Assert
        result.Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be("test");
    }

    [Fact]
    public void WithOneTab_WhenLayout_ThenAddsTwoSpaces()
    {
        // Act
        var result = _display.Layout(new List<FormattedText>() { "test".FormatChunk() }, 1);

        // Assert
        result.Chunks.Aggregate("", (a, b) => $"{a}{b.RawText}").Should().Be(" test");
    }
}

