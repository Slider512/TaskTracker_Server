using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    [Table("BC_TaskLinks")]
    [PrimaryKey(nameof(CompanyId), nameof(ProjectId), nameof(Id))]
    public class ProjectTaskDependency
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }

        public Guid CompanyId { get; set; }

        [Required]
        public Guid FromTaskId { get; set; }

        [Required]
        public Guid ToTaskId { get; set; }

        [MaxLength(2), Required]
        public string Type { get; set; } // FS, FF, SS, SF

        [ForeignKey($"{nameof(CompanyId)},{nameof(ProjectId)},{nameof(FromTaskId)}")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ProjectTask FromTask { get; set; }

        [ForeignKey($"{nameof(CompanyId)},{nameof(ProjectId)},{nameof(ToTaskId)}")]
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public virtual ProjectTask ToTask { get; set; }
    }
}
