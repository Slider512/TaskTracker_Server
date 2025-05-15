using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; } // Например, "Executor", "ScrumMaster", "ProjectManager"
        [Required]
        public Guid CompanyId { get; set; }

        [ForeignKey(nameof(User.CompanyId))]
        public Project Company { get; set; }

        public List<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
