using FluentAssertions;
using clocknet.systemtests;

Console.WriteLine("=== Starting System Tests ===");

BasicTests.WithNoSettings_WithNoClock_WhenAddStopShow_ThenCreatesFileAndShowsOutput();
BasicTests.WithNoClock_WhenAddStopShow_ThenCreatesFileAndShowsOutput();
