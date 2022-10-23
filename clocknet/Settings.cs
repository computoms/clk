using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace clocknet;

public record Settings
{
	public string File { get; set; } = "~/clock.txt";
	public string DefaultTask { get; set; } = "Started empty task";

    public static Settings Read()
    { 
		var homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		var settingsFile = Path.Combine(homePath, ".clock", "settings.yml");
		var mainFile = Path.Combine(homePath, "clock.txt");
		if (System.IO.File.Exists(settingsFile))
		{
			try
			{
				return ParseSettings(settingsFile);
			}
			catch (Exception e)
			{
				Console.WriteLine("Error while reading settings file: " + e.Message);
			}
		}

		return new Settings { File = mainFile };
    }

	private static Settings ParseSettings(string filename)
	{
		var deserializer = new DeserializerBuilder()
			.Build();

        return deserializer.Deserialize<Settings>(System.IO.File.ReadAllText(filename));
    }
}

