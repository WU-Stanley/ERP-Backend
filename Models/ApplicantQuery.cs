using System.ComponentModel.DataAnnotations;

namespace WUIAM.Models
{
    public class ApplicantQuery
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ApplicationId { get; set; }
        public JobApplication? Application { get; set; }

        [Required, MaxLength(5000)]
        public string Message { get; set; } = string.Empty;

        [MaxLength(50)]
        public string MessageFrom { get; set; } = "Applicant";

        public Guid? FromUserId { get; set; }
        public User? FromUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
