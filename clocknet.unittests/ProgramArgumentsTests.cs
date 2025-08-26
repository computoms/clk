
using clocknet;
using FluentAssertions;

public class ProgramArgumentsTests
{
    [Fact]
    public void WithMultipleArguments_WhenGetValue_ThenReturnsCorrectValue()
    {
        // Arrange
        var pArgs = new ProgramArguments(["show", "--bar", "--tags", "tags", "--all"]);

        // Act
        var value = pArgs.GetValue(Args.Tags);

        // Assert
        value.Should().Be("tags");
    }
}