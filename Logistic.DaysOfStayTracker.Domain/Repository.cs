using Logistic.DaysOfStayTracker.Core.Common;
using Microsoft.EntityFrameworkCore;

namespace Logistic.DaysOfStayTracker.Core;

public class Repository<T> where T : Entity
{
    private readonly AppDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(AppDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    public async Task<T> DeleteAsync(Guid id)
    {
        var entity = await _set.FindAsync(id);
        if (entity == null)
            throw new ArgumentException("Not found", nameof(id));
        
        _set.Remove(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}