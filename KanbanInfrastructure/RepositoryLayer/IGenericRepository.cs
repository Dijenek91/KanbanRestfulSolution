namespace KanbanInfrastructure.RepositoryLayer
{
    public interface IGenericRepository<TEntity> where TEntity : class
    {
        public IQueryable<TEntity> GetQueryableEntities(); //used for filtering, sorting, and pagination

        Task<IEnumerable<TEntity>> GetAllRecordsAsync();

        Task<TEntity> FindAsync(int? entityId);

        void Add(TEntity entity);

        void Update(TEntity entity);

        void Delete(TEntity entity);

        void DetachEntry(TEntity entity);

        void SetOriginalValueRowVersion(TEntity entity, byte[] rowVersion);

        IQueryable<TEntity> GetTable();
    }
}
