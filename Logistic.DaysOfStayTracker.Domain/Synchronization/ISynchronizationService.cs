﻿using CSharpFunctionalExtensions;

namespace Logistic.DaysOfStayTracker.Core.Synchronization;

public interface ISynchronizationService
{
    /// <summary>
    /// Возвращает дату последнего изменений файла БД на сервере
    /// </summary>
    /// <returns>null, если там файла нет</returns>
    DateOnly? GetDbModificationDate();

    /// <summary>
    /// Получает актуальный файл БД из сервера
    /// </summary>
    Task<Result<AppDbContext>> GetDb();
    
    /// <summary>
    /// Загружает новый файл БД на сервер
    /// </summary>
    Task<Result> SaveDb(AppDbContext db);
}