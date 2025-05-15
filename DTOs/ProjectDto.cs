using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Server.DTOs
{
    public class ProjectDto
    {
        public Guid Id { get; set; }

        public Guid CompanyId { get; set; }

        public string Name { get; set; }
    }
}
