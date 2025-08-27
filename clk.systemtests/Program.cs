using FluentAssertions;
using clk.systemtests;

Console.WriteLine("=== Starting System Tests ===");

BasicTests.WithNoSettings_WithNoClock_WhenAddStopShow_ThenCreatesFileAndShowsOutput();
BasicTests.WithNoClock_WhenAddStopShow_ThenCreatesFileAndShowsOutput();
