using clk.Utils;

namespace clk.Domain.Reports;

internal class JsonReport(IDisplay display) : IReport
{
    public string Name { get; } = Args.Json;

    public void Print(IEnumerable<Activity> activities)
    {
        var records = activities.SelectMany(a => a.Records.Select(r => new SingleRecord(r, a)))
            .OrderBy(r => r.Record.StartTime);

        display.Print(records.Select(LayoutSingleRecord));
    }


    private FormattedLine LayoutSingleRecord(SingleRecord record)
    {
        var paths = string.Join(",", record.Activity.Task.Path.Select(p => $"\"{p}\""));
        var tags = string.Join(",", record.Activity.Task.Tags.Select(t => $"\"{t}\""));
        var line = new List<FormattedText>(){
            "{ ".FormatChunk(),
            $"\"Title\": \"{record.Activity.Task.Title}\", ".FormatChunk(),
            $"\"Date\": \"{record.Record.StartTime.Date:yyyy-MM-dd}\", ".FormatChunk(),
            $"\"Start\": \"{record.Record.StartTime:HH:mm:ss}\", ".FormatChunk(),
            $"\"End\": \"{record.Record.EndTime:HH:mm:ss}\", ".FormatChunk(),
            $"\"Id\": \"{record.Activity.Task.Id}\", ".FormatChunk(),
            $"\"Paths\": [{paths}], ".FormatChunk(),
            $"\"Tags\": [{tags}] ".FormatChunk(),
            "}".FormatChunk(),
        };
        return display.Layout(line);
    }

    private record SingleRecord(Record Record, Activity Activity);
}