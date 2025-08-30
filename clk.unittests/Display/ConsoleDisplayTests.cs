using clk.Domain;
using clk.Infra;
using FluentAssertions;

namespace clk.unittests;

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

    [Theory]
    [InlineData(5, 5, "Text2")]
    [InlineData(0, 5, "Text1")]
    [InlineData(10, 5, "Text3")]
    [InlineData(3, 6, "t1Text")]
    [InlineData(3, 1, "t")]
    [InlineData(3, 10, "t1Text2Tex")]
    [InlineData(3, 12, "t1Text2Text3")]
    public void FormattedLine_Substring(int startIndex, int count, string expectedText)
    {
        // Arrange
        var line = new FormattedLine(
            [
                new FormattedText("Text1"),
                new FormattedText("Text2"),
                new FormattedText("Text3")
            ]
        );

        // Act
        var result = line.Substring(startIndex, count);

        // Assert
        result.Chunks.Aggregate("", (t, c) => $"{t}{c.RawText}").Should().Be(expectedText);
    }
}

