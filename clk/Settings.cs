using YamlDotNet.Serialization;

namespace clk;

public record Settings
{
	public SettingsData Data { get; set; } = new SettingsData();

	private static string[] _defaultSettingsFilename = ["settings.yml", "settings.yaml"];

	public Settings(ProgramArguments pArgs)
	{
		if (pArgs.HasOption(Args.Settings) && Read(pArgs.GetValue(Args.Settings)))
		{
			return;
		}

		var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var defaultSettings = _defaultSettingsFilename.Select(x => Path.Combine(homePath, ".clk", x)).FirstOrDefault(x => File.Exists(x));
		if (defaultSettings != null && Read(defaultSettings))
			return;

		Data.File = Path.Combine(homePath, "clock.txt");
	}

	private bool Read(string filename)
	{
		if (!File.Exists(filename))
		{
			Console.WriteLine($"Settings file does not exist: {filename}");
			return false;
		}

		try
		{
			Data = ParseSettings(filename);
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error while reading settings file: {e.Message} {e.StackTrace}");
			return false;
		}

		return true;
	}

	private SettingsData ParseSettings(string filename)
	{
		var deserializer = new DeserializerBuilder()
			.Build();

		var data = deserializer.Deserialize<SettingsData>(File.ReadAllText(filename));
		return data;
	}

	public class SettingsData
	{
		public SettingsData()
		{
			var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			File = Path.Combine(homePath, "clock.txt");
		}

		public string File { get; set; }
		public string DefaultTask { get; set; } = "Started empty task";
		public string EditorCommand { get; set; } = "code";
	}
}

