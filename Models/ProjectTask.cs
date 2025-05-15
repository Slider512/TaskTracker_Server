using Microsoft.AspNetCore.Components;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class ProjectTask
    {
        [Key]
        public Guid Id { get; set; }
        [Key]
        public Guid ProjectUid { get; set; }

        [Key]
        public Guid CompanyId { get; set; }

        [MaxLength(4000), Required]
        public string Title { get; set; } = "";

        [MaxLength(4000)]
        public string Description { get; set; } = "";
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; } = 0;
        public List<User> AssignedUsers { get; set; }

        [MaxLength(255)]
        public string? Status { get; set; }
        public int Priority { get; set; } = 1000;
        public Guid? ParentTaskId { get; set; }

        [ForeignKey(nameof(ProjectTask.ParentTaskId))]
        public virtual ProjectTask? ParentTask { get; set; }
        public virtual ICollection<ProjectTask> Subtasks { get; set; } = new List<ProjectTask>();
        //public virtual ICollection<ProjectTaskDependency> Dependencies { get; set; }

        public Guid ProjectId { get; set; }

        [ForeignKey(nameof(ProjectTask.ProjectId))]
        public Project Project { get; set; }
        public int? EffortHours { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [InverseProperty(nameof(ProjectTaskDependency.ToTask))]
        public List<ProjectTaskDependency> IncomingDependencies { get; set; } = new List<ProjectTaskDependency>();

        [InverseProperty(nameof(ProjectTaskDependency.FromTask))]
        public List<ProjectTaskDependency> OutgoingDependencies { get; set; } = new List<ProjectTaskDependency>();
    }
}
