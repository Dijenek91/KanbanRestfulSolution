using KanbanInfrastructure.DAL;
using KanbanInfrastructure.RepositoryLayer.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace KanbanInfrastructure.RepositoryLayer
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity>, IDisposable
       where TEntity : class
    {
        private DbSet<TEntity> _entities;        
        private bool _isDisposed;

        public DbContext Context { get; set; }

        public GenericRepository(IUnitOfWork<KanbanAppDbContext> unitOfWork) 
        {
            _isDisposed = false;
            Context = unitOfWork.Context;
        }

        public virtual IQueryable<TEntity> GetTable()
        {
            return Entities;
        }

        public virtual void DetachEntry(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Detached;
        }

        public virtual void SetOriginalValueRowVersion(TEntity entity, byte[] rowVersion)
        {
            Context.Entry(entity).OriginalValues["RowVersion"] = rowVersion;
        }

        protected virtual DbSet<TEntity> Entities
        {
            get { return _entities ?? (_entities = Context.Set<TEntity>()); }
        }

        public IQueryable<TEntity> GetQueryableEntities()
        {
            return Entities.AsQueryable();
        }

        public async virtual Task<IEnumerable<TEntity>> GetAllRecordsAsync()
        {
            return await Entities.ToListAsync();
        }

        public async virtual Task<TEntity> FindAsync(int? id)
        {
            if(id == null)
                throw new ArgumentNullException(nameof(id));
            
            return await Entities.FindAsync(id);
        }

        public virtual void Add(TEntity entity)
        {
            IsEntityNull(entity);            

            ValidateEntityAndThrowException(entity);

            Entities.Add(entity);      
        }  

        public void BulkInsert(IEnumerable<TEntity> entities)
        {
            var previousAutoDetect = Context.ChangeTracker.AutoDetectChangesEnabled;

            AreEntitiesNull(entities);
            
               
            foreach (var entity in entities)
            {
                ValidateEntityAndThrowException(entity);
            }

            //disabling autodetect since we are adding multiple entities and that slows down performance of EF
            //only disabled during add of of each entity in the collection
            //not so much applicable for addRange method, but kept just in case this part of code is modified in future
            Context.ChangeTracker.AutoDetectChangesEnabled = false;
            
            Context.Set<TEntity>().AddRange(entities);

            Context.ChangeTracker.AutoDetectChangesEnabled = previousAutoDetect;
        }

        public virtual void Update(TEntity entity)
        {
            IsEntityNull(entity);
            
            ValidateEntityAndThrowException(entity);
            SetEntryModified(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            IsEntityNull(entity);
            
            ValidateEntityAndThrowException(entity);
            if (Context.Entry(entity).State == EntityState.Detached)
            {
                Entities.Attach(entity);
            }
            Entities.Remove(entity);
        }

        public virtual void SetEntryModified(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public void Dispose()
        {
            if (Context != null)
            {
                Context.Dispose();
            }
            _isDisposed = true;
        }

        #region Private helper methods

        /// <summary>
        /// Validates entity and throws exception if validation fails
        /// </summary>
        /// <param name="entity">
        /// Entity parameter that will be checked
        private static void ValidateEntityAndThrowException(TEntity entity)
        {
            try
            {
                // Perform manual validation if needed
                var validationContext = new ValidationContext(entity);
                var validationResults = new List<ValidationResult>();

                bool isValid = Validator.TryValidateObject(entity, validationContext, validationResults, validateAllProperties: true);

                if (!isValid)
                {
                    var errorMessage = string.Join(Environment.NewLine,
                        validationResults.Select(vr => $"Property: {string.Join(",", vr.MemberNames)} Error: {vr.ErrorMessage}"));

                    throw new ValidationException(errorMessage);
                }
            }
            catch (ValidationException ex)
            {
                throw new Exception("Validation failed: " + ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new Exception("Gen exception occured during validation: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Checks if entity is null
        /// </summary>
        /// <param name="entity">
        /// Entity parameter that will be checked
        /// <throw>
        /// If entity is null, this method will throw ArgumentNullException
        /// </throw>
        private static void IsEntityNull(TEntity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
        }

        /// <summary>
        /// Checks if entities(plural) are null
        /// </summary>
        /// <param name="entities">
        /// IEnumrable entities parameter that will be verified
        /// </param>
        /// <throw>
        /// If entity is null, this method will throw ArgumentNullException
        /// </throw>
        private static void AreEntitiesNull(IEnumerable<TEntity> entities)
        {
            if (entities == null)
                throw new ArgumentNullException("entities");
        }

       
        #endregion

    }
}
