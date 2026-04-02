namespace KanbanModel.DTOs.ResponseDTOs
{
    //sorting format example is: ?sort=field1,asc&sort=field2,desc

    public class PagedResultKanbanTasksResponse<TEntity>
    {
        public List<TEntity> Items { get; }
        public int TotalCount { get; }
        public int Page { get; }
        public int Size { get; }

        public PagedResultKanbanTasksResponse(List<TEntity> items, int totalCount, int page, int size)
        {
            Items = items;
            TotalCount = totalCount;
            Page = page;
            Size = size;
        }

        // HATEOAS links
        public List<LinkDTO> Links { get; set; } = new();
    }
}
