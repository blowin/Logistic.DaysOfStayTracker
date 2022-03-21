using X.PagedList;

namespace Logistic.DaysOfStayTracker.Core.Extension;

public static class QueryableExt
{
    public static Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> self, int page, CancellationToken cancellationToken = default) 
        => self.ToPagedListAsync(page, Constants.DefaultPageSize, cancellationToken);
}