namespace clocknet.systemtests;

public static class BasicTests
{
    public static void WithNoSettings_WithNoClock_WhenAddStopShow_ThenCreatesFileAndShowsOutput()
    { 
		Console.WriteLine("- No settings - no clock.txt");
		var currentTime = DateTime.Now.ToString("HH:mm");
		Runner _runner = new();
		_ = _runner.Run("add Test1 +tag .123");
		_ = _runner.Run($"add --at {currentTime} Test2 .345");
		_ = _runner.Run($"add --at {currentTime} .123");
		_ = _runner.Run("stop");
		var output = _runner.Run("show");

		var expected = new List<string>()
		{
			DateTime.Now.Date.ToString("yyyy-MM-dd"),
			" 00:00 Test1 +tag .123",
			$"   00:00 ({currentTime} -> {currentTime})",
			$"   00:00 ({currentTime} -> {currentTime})",
			" 00:00 Test2 .345",
			$"   00:00 ({currentTime} -> {currentTime})",
			" ",
			"00:00 Total",
		};

		Test.Expect(expected, output);
		Test.RemoveClock();
    }

	public static void WithNoClock_WhenAddStopShow_ThenCreatesFileAndShowsOutput()
	{
		Console.WriteLine("- With settings - without clock1.txt");
		Test.EnableSettings();

		Runner _runner = new();
		_ = _runner.Run("add Test1 +tag .123");
		_ = _runner.Run("add");
		_ = _runner.Run("stop");
		var output = _runner.Run("show");

		var currentTime = DateTime.Now.ToString("HH:mm");
		var expected = new List<string>()
		{
			DateTime.Now.Date.ToString("yyyy-MM-dd"),
			" 00:00 Test1 +tag .123",
			$"   00:00 ({currentTime} -> {currentTime})",
			" 00:00 none",
			$"   00:00 ({currentTime} -> {currentTime})",
			" ",
			"00:00 Total",
		};

		Test.Expect(expected, output);
		Test.RemoveClock("/home/systemtest/clock1.txt");
	}
}

