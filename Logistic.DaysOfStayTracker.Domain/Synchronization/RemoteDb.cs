namespace Logistic.DaysOfStayTracker.Core.Synchronization;

public class RemoteDb
{
    public string FilePath { get; }
    public AppDbContext Database { get; }

    public RemoteDb(string filePath, AppDbContext database)
    {
        FilePath = filePath;
        Database = database;
    }
}