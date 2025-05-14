using Microsoft.AspNetCore.Identity;

namespace Server.Models
{
    public class User : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Role { get; set; } // Например, "Executor", "ScrumMaster", "ProjectManager"
        public string? CompanyName { get; set; } // Название компании
    }
}
