using Microsoft.AspNetCore.Http;
using Server.Models;
using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class TaskDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        [StringLength(4000)]
        public string Title { get; set; } = string.Empty;
        public List<ProjectResource> AssignedUsers { get; set; }
        [StringLength(255)]
        public string? Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Progress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? ParentId { get; set; }
        public List<Guid> Subtasks { get; set; } = new List<Guid>();
        public List<DependencyDto> Dependencies { get; set; } = new List<DependencyDto>();
    }

    public class DependencyDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public Guid FromTaskId { get; set; } = Guid.Empty;
        public Guid ToTaskId { get; set; } = Guid.Empty;
        public string Type { get; set; } = "FS";
    }
}
