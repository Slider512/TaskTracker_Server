using Aspose.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    [Table("BC_Resources")]
    [PrimaryKey(nameof(CompanyId), nameof(ProjectUid), nameof(Id))]
    public class ProjectResource 
    {
        public Guid Id { get; set; }

        public Guid ProjectUid { get; set; }

        [ForeignKey($"{nameof(CompanyId)},{nameof(ProjectUid)}")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Project Project { get; set; }

        public Guid CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }


        public string? Email { get; set; }
        [Required]
        public string Title { get; set; }

        public string? UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
