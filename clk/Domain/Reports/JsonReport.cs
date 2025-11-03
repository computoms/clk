using clk.Utils;

namespace clk.Domain.Reports;

internal class JsonReport(IDisplay display) : IReport
{
    public string Name { get; } = Args.Json;

    public void Print(IEnumerable<TaskLine> tasks)
    {
        var records = tasks.OrderBy(t => t.StartTime);
        display.Print(records.Select(LayoutSingleRecord));
    }


    private FormattedLine LayoutSingleRecord(TaskLine task)
    {
        var paths = string.Join(",", task.Path.Select(p => $"\"{p}\""));
        var tags = string.Join(",", task.Tags.Select(t => $"\"{t}\""));
        var line = new List<FormattedText>(){
            "{ ".FormatChunk(),
            $"\"Title\": \"{task.Title}\", ".FormatChunk(),
            $"\"Date\": \"{task.StartTime.Date:yyyy-MM-dd}\", ".FormatChunk(),
            $"\"Start\": \"{task.StartTime:HH:mm:ss}\", ".FormatChunk(),
            $"\"End\": \"{task.EndTime:HH:mm:ss}\", ".FormatChunk(),
            $"\"Id\": \"{task.Id}\", ".FormatChunk(),
            $"\"Paths\": [{paths}], ".FormatChunk(),
            $"\"Tags\": [{tags}] ".FormatChunk(),
            "}".FormatChunk(),
        };
        return display.Layout(line);
    }

    private record SingleRecord(Record Record, Activity Activity);
}