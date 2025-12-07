using Microsoft.EntityFrameworkCore;

namespace KanbanInfrastructure.RepositoryLayer.UnitOfWork
{
    public delegate void VerifyEntityAndSetRowVersion<TEntity>(TEntity databaseEntityValues, TEntity uiFilledEntityValues, TEntity returnModel);

    public interface IUnitOfWork<out TContext> : IDisposable
        where TContext : DbContext
    {
        TContext Context { get; }

        IGenericRepository<TEntity> GenericRepository<TEntity>() where TEntity : class;

        Task SaveAsync();

        Task<(bool Success, string ErrorMessage)> Save<TEntity>(TEntity entityToUpdate, VerifyEntityAndSetRowVersion<TEntity> verifyEntityAndSetRowVersionFunc)
            where TEntity : class;
    }
}
