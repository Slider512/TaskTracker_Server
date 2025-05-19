using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;

namespace Server.Models
{
    [Table("BC_Projects")]
    [PrimaryKey(nameof(Id), nameof(CompanyId))]
    public class Project
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }

        [Required]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public List<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
    }
}
