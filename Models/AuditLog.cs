using System.ComponentModel.DataAnnotations;

namespace Security_Bug_Reports.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Details { get; set; }

    }
}
