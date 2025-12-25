using Microsoft.EntityFrameworkCore;
using System.Threading;

namespace KanbanInfrastructure.RepositoryLayer.UnitOfWork
{
    public delegate void VerifyEntityAndSetRowVersion<TEntity>(TEntity databaseEntityValues, TEntity uiFilledEntityValues, TEntity returnModel);

    public interface IUnitOfWork<out TContext> : IDisposable
        where TContext : DbContext
    {
        TContext Context { get; }

        IGenericRepository<TEntity> GenericRepository<TEntity>() where TEntity : class;

        Task SaveAsync(CancellationToken cancellationToken);

        Task<(bool Success, string ErrorMessage)> Save<TEntity>(TEntity entityToUpdate, VerifyEntityAndSetRowVersion<TEntity> verifyEntityAndSetRowVersionFunc, CancellationToken cancellationToken)
            where TEntity : class;
    }
}
