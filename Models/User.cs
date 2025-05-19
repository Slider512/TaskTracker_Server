using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    [Table("BC_Users", Schema = "bst")]
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; } // Например, "Executor", "ScrumMaster", "ProjectManager"

        [Required]
        public Guid CompanyId { get; set; }

        [ForeignKey(nameof(CompanyId))]
        public Company Company { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
