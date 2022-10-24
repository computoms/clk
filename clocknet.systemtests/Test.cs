using FluentAssertions;

namespace clocknet.systemtests;

public static class Test
{
    public static void RemoveClock(string path = null)
    {
        if (path == null)
            path = "/home/systemtest/clock.txt";

        System.IO.File.Delete(path);
    }

    public static void DisableSettings()
    {
        System.IO.File.Move("/home/systemtest/.clock/settings.yml", "/home/systemtest/.clock/settings.yml.disabled");
    }

    public static void EnableSettings()
    {
        System.IO.File.Move("/home/systemtest/.clock/settings.yml.disabled", "/home/systemtest/.clock/settings.yml");
    }

    public static void Expect(IReadOnlyList<string> expected, IReadOnlyList<string> actual)
    {
        Console.WriteLine("      Expected: ");
        foreach (var ex in expected)
        {
            Console.WriteLine($"      - {ex}");
	    }

        Console.WriteLine("      Actual:");
        actual.Should().HaveCount(expected.Count);
        foreach (var value in actual.Zip(expected))
        {
            Console.WriteLine($"      - {value.First}");
            value.First.Should().Be(value.Second);
	    }
        Console.WriteLine("      Passed.");
    }
}

