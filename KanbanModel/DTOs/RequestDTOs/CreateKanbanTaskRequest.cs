using KanbanModel.ModelClasses;
using System.ComponentModel.DataAnnotations;

namespace KanbanModel.DTOs.RequestDTOs
{
    public class CreateKanbanTaskRequest
    {

        [Required(ErrorMessage = "Task name is required")]
        [StringLength(100, ErrorMessage = "Task name cannot exceed 100 characters")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Status is required")]
        [Range(1, 3, ErrorMessage = "Status must be between 1 and 3 (ToDo-InProgress-Done)")]
        public StatusEnum Status { get; set; }

        [Range(1, 100, ErrorMessage = "Size must be between 1 and 100")]
        public int? Size { get; set; }

        [Range(1, 3, ErrorMessage = "Priority must be between 1 and 3 (Low-Med-High)")]
        public PriorityEnum? PriorityEnum { get; set; }


        
    }
}
