
using clk;
using FluentAssertions;
using YamlDotNet.Serialization;

public class SettingsTests
{
    [Fact]
    public void DeserializerTest()
    {
        var deserializer = new DeserializerBuilder()
            .Build();

        var result = deserializer.Deserialize<Settings.SettingsData>("File: /Users/thomas/clock.txt\nDefaultTask: Test\nEditorCommand: code");

        result.File.Should().Be("/Users/thomas/clock.txt");
    }
}