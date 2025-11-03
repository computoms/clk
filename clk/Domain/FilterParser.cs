using System.Net.Quic;
using clk.Utils;

namespace clk.Domain;

public class FilterParser(ProgramArguments pArgs, IRecordRepository recordRepository, ITimeProvider timeProvider)
{
    public IEnumerable<TaskLine> Filter()
    {
        RepositoryQuery query = new RepositoryQuery();
        if (pArgs.HasOption(Args.All))
        { }
        else if (pArgs.HasOption(Args.Week))
            query = query with { From = timeProvider.Now.MondayOfTheWeek(), To = timeProvider.Now.MondayOfTheWeek().AddDays(4) };
        else if (pArgs.HasOption(Args.Yesterday))
            query = query with { From = timeProvider.Now.Date.AddDays(-1), To = timeProvider.Now.Date.AddDays(-1) };
        else
            query = query with { From = timeProvider.Now.Date, To = timeProvider.Now.Date };

        if (pArgs.HasOption(Args.Tags))
        {
            var tags = pArgs.GetValue(Args.Tags).Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            query = query with { Tags = tags };
        }
        if (pArgs.HasOption(Args.Path))
        {
            var path = pArgs.GetValue(Args.Path).Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
            query = query with { Path = path };
        }

        if (pArgs.HasOption(Args.Last))
        {
            query = query with { Last = int.Parse(pArgs.GetValue(Args.Last)) };
        }

        return recordRepository.FilterByQuery(query);
    }
}