using System.ComponentModel.DataAnnotations.Schema;

namespace Server.Models
{
    [Table("BC_AuditLogs")]
    public class AuditLog
    {
        public int Id { get; set; }
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
