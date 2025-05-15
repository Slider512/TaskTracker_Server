using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class ProjectTaskDependency
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid FromTaskId { get; set; }

        [Required]
        public Guid ToTaskId { get; set; }

        [MaxLength(2), Required]
        public string Type { get; set; } // FS, FF, SS, SF
        public virtual ProjectTask FromTask { get; set; }
        public virtual ProjectTask ToTask { get; set; }
    }
}
