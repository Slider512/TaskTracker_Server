using System.ComponentModel.DataAnnotations;

namespace Server.Models
{
    public class Company
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }
    }
}
