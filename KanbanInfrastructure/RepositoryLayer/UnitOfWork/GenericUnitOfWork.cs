using Microsoft.EntityFrameworkCore;

namespace KanbanInfrastructure.RepositoryLayer.UnitOfWork
{
    public class GenericUnitOfWork<TContext> : IUnitOfWork<TContext>
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private bool _disposed;
        private Dictionary<string, object> _repositories;

        public GenericUnitOfWork(TContext context)
        {
            _dbContext = context;
        }
       
        public TContext Context
        {
            get
            {
                return _dbContext;
            }
        }

        public IGenericRepository<T> GenericRepository<T>()
           where T : class
        {
            if (_repositories == null)
            {
                _repositories = new Dictionary<string, object>();
            }


            var type = typeof(T).Name;
            if (!_repositories.ContainsKey(type))
            {
                var repositoryType = typeof(GenericRepository<>);
                var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);
                _repositories.Add(type, repositoryInstance);
            }
            return (GenericRepository<T>)_repositories[type];
        }


        public async Task SaveAsync()
        {
            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new Exception("Database update failed: " + ex.Message, ex);
            }

        }

        //save that checks concurrency 
        public async Task<(bool Success, string ErrorMessage)> Save<TEntity>(TEntity entityToUpdate, VerifyEntityAndSetRowVersion<TEntity> verifyEntityAndSetRowVersionFunc)
            where TEntity : class
        {
            var errorMessage = string.Empty;

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                var entry = ex.Entries.Single();
                var databaseValues = await entry.GetDatabaseValuesAsync();

                if (databaseValues == null)
                {
                    errorMessage = "The entity was deleted by another user.";
                    return (false, errorMessage);
                }

                var dbEntity = (TEntity) databaseValues.ToObject();
                var uiEntity = (TEntity) entry.Entity;

                verifyEntityAndSetRowVersionFunc(dbEntity, uiEntity, entityToUpdate);
                return (false, errorMessage);
            }

            return (true, errorMessage);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
            }

            _disposed = true;
        }

    }
}
