using Moq;
using Moq.AutoMock;

namespace clocknet.unittests;

public class RecordRepositoryTests
{
    private readonly AutoMocker _mocker = new();

    [Fact]
    public void WhenAddingEntry_ThenEntryIsAddedToStorage()
    {
        // Arrange
        var repository = _mocker.CreateInstance<RecordRepository>();
        var activity = new Activity(0, "Test", new string[0], "");
        var record = new Record(0, DateTime.Now, null);

        // Act
        repository.AddRecord(activity, record);

        // Assert
        _mocker.GetMock<IStorage>().Verify(x => x.AddEntry(activity, record), Times.Once);
    }
}
