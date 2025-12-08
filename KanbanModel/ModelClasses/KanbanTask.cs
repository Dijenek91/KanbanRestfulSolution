using System.ComponentModel.DataAnnotations;

namespace KanbanModel.ModelClasses
{

    // size, enums are nullable to support partial updates
    public class KanbanTask
    {        
        public int Id { get; set; }

        public string Name { get; set; }
        
        public string Description { get; set; }
      
        public StatusEnum? Status { get; set; }

        public int? Size { get; set; }
        
        public PriorityEnum? PriorityEnum { get; set; }
    }
}
