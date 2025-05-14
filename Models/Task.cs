using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class Task
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public string Assignee { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public Guid? ParentTaskId { get; set; }

        [ForeignKey("ParentTaskId")]
        public virtual Task ParentTask { get; set; }
        public virtual ICollection<Task> Subtasks { get; set; } = new List<Task>();
        public virtual ICollection<Dependency> Dependencies { get; set; }
        public string? ProjectId { get; set; }
        public int? EffortHours { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [InverseProperty("ToTask")]
        public List<Dependency> IncomingDependencies { get; set; } = new List<Dependency>();

        [InverseProperty("FromTask")]
        public List<Dependency> OutgoingDependencies { get; set; } = new List<Dependency>();
    }

    public class Dependency
    {
        public Guid Id { get; set; }
        public Guid FromTaskId { get; set; }
        public Guid ToTaskId { get; set; }
        public string Type { get; set; } // FS, FF, SS, SF
        public virtual Task FromTask { get; set; }
        public virtual Task ToTask { get; set; }
    }
}
