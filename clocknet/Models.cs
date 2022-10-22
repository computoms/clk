namespace clocknet;

public record Activity(long Id, string Title, string[] Tags, string Number)
{ 
    public bool IsSameAs(string title, string[] tags, string number)
    { 
        return title == Title && number == Number
            && Tags.All(x => tags.Contains(x))
            && tags.All(x => Tags.Contains(x));
    }
}

public record Record(long ActivityId, DateTime StartTime, DateTime? EndTime);