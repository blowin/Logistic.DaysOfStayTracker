using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core;

public class Constants
{
    public const int DefaultPageSize = 10;

    public static IPagedList<T> CreateEmptyPagedList<T>()
        => new StaticPagedList<T>(Enumerable.Empty<T>(), 1, DefaultPageSize, 0);
}