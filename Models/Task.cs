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
        public virtual Task ParentTask { get; set; }
        public virtual ICollection<Task> Subtasks { get; set; }
        public virtual ICollection<Dependency> Dependencies { get; set; }
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
