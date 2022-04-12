using CSharpFunctionalExtensions;

namespace Logistic.DaysOfStayTracker.Core.Synchronization;

public interface ISynchronizationService : IDisposable
{
    /// <summary>
    /// Возвращает дату последнего изменений файла БД на сервере
    /// </summary>
    /// <returns>null, если там файла нет</returns>
    Task<Result<DateOnly?>> GetDbModificationDate();

    /// <summary>
    /// Получает актуальный файл БД из сервера
    /// </summary>
    Task<Result<AppDbContext>> GetDb();
    
    /// <summary>
    /// Загружает новый файл БД на сервер
    /// </summary>
    Task<Result> SaveDb(AppDbContext db);
}