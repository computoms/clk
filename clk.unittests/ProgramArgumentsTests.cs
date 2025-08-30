
using clk;
using FluentAssertions;
using Xunit.Sdk;

public class ProgramArgumentsTests
{
    [Fact]
    public void WithStandardAddCommand_WhenParsing_ThenExtractsArguments()
    {
        // Arrange & Act
        var pArgs = new ProgramArguments(["add", "--at", "10:00", "This", "is", "a", "test", "+tag", ".01"]);

        // Assert
        pArgs.Title.Should().Be("This is a test +tag .01");
        pArgs.Time.Hour.Should().Be(10);
        pArgs.Time.Minute.Should().Be(0);
    }

    [Theory]
    [InlineData("+", "Title + other")]
    [InlineData(".", "Title . other")]
    public void WithInvalidTagOrId_WhenParsing_ThenKeepsArgsInTitle(string testWord, string expectedTitle)
    {
        // Arrange & Act
        var pArgs = new ProgramArguments(["add", "Title", testWord, "other"]);

        // Assert
        pArgs.Title.Should().Be(expectedTitle);
    }

    [Fact]
    public void WithMultipleArguments_WhenGetValue_ThenReturnsCorrectValue()
    {
        // Arrange
        var pArgs = new ProgramArguments(["show", "--bar", "--group-by", "tags", "--all"]);

        // Act
        var value = pArgs.GetValue(Args.GroupBy);

        // Assert
        value.Should().Be("tags");
    }

    [Fact]
    public void WithAtOption_WhenParsing_ThenGetsTime()
    {
        // Arrange & Act
        var pArgs = new ProgramArguments(["add", "--at", "10:11", "Test"]);

        // Assert
        pArgs.Time.Hour.Should().Be(10);
        pArgs.Time.Minute.Should().Be(11);
    }

    [Fact]
    public void WithCommand_WhenParsing_ThenGetsCommand()
    {
        // Arrange & Act
        var pArgs = new ProgramArguments(["add", "--at", "10:11", "Test"]);

        // Assert
        pArgs.Command.Should().Be("add");
    }
}