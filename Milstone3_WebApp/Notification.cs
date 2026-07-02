using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Milstone3_WebApp
{
    public partial class Notification
    {
        [Key]
        [Column("notification_id")]
        public int NotificationId { get; set; }

        [Required(ErrorMessage = "Notification type is required")]
        [Column("notification_type")]
        [StringLength(50)]
        public string NotificationType { get; set; } = null!;

        // Better to store timestamp as DateTime instead of TimeSpan
        [Column("timestamp")]
        [DataType(DataType.DateTime)]
        public DateTime? Timestamp { get; set; }

        [Column("urgency")]
        [StringLength(20)]
        public string? Urgency { get; set; }

        [Column("read_status")]
        [StringLength(20)]
        public string? ReadStatus { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [Column("title")]
        [StringLength(100)]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "Message content is required")]
        [Column("message_content")]
        [StringLength(500)]
        public string MessageContent { get; set; } = null!;

        [Column("priority")]
        [StringLength(20)]
        public string? Priority { get; set; }

        [Column("created_at")]
        [DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }

        [Column("expires_at")]
        [DataType(DataType.DateTime)]
        public DateTime? ExpiresAt { get; set; }

        public virtual ICollection<EmployeeNotification> EmployeeNotifications { get; set; } = new List<EmployeeNotification>();
    }
}
