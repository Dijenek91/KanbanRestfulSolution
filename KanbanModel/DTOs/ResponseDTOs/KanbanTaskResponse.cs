using KanbanModel.ModelClasses;

namespace KanbanModel.DTOs.ResponseDTOs
{
    public class KanbanTaskResponse
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public StatusEnum? Status { get; set; }

        public int? Size { get; set; }

        public PriorityEnum? PriorityEnum { get; set; }

        // HATEOAS links
        public List<LinkDTO> Links { get; set; } = new();
    }
}
