
using System.Diagnostics;

namespace clk.systemtests;

public class Runner
{
    private readonly string _filename = "/home/systemtest/bin/clk.dll";

    public IReadOnlyList<string> Run(string arguments)
    {
        Console.WriteLine($"    Running {arguments}");
        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = string.Join(' ', _filename, arguments),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        var lines = new List<string>();
        proc.Start();
        while (!proc.StandardOutput.EndOfStream || !proc.StandardError.EndOfStream)
        {
            var std = proc.StandardOutput.ReadLine();
            var err = proc.StandardError.ReadLine();
            if (std != null)
			    lines.Add(std);
            if (err != null)
                lines.Add(err);
        }
        proc.WaitForExit();
        return lines;
    }
}

