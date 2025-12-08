using KanbanModel.ModelClasses;
using System.ComponentModel.DataAnnotations;

namespace KanbanModel.DTOs
{
    public class FullUpdateKanbanTaskDTO
    {
        [Required(ErrorMessage = "Task name is required")]
        [StringLength(100, ErrorMessage = "Task name cannot exceed 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Range(1, 3, ErrorMessage = "Status must be between 1 and 3 (ToDo-InProgress-Done)")]
        public StatusEnum Status { get; set; }

        [Required(ErrorMessage = "Size is required in full task update operation")]
        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int? Size { get; set; }

        [Required(ErrorMessage = "Priority is required in full task update operation")]
        [Range(1, 3, ErrorMessage = "Priority must be between 1 and 3 (Low-Med-High)")]
        public PriorityEnum PriorityEnum { get; set; }
    }
}
