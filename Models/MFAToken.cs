using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace WUIAM.Models
{
    public class MFAToken
    {
        
            [Key]
            public Guid Id { get; set; }
            public string Token { get; set; }
            public DateTime ExpiresOn { get; set; }
            public Guid? UserId { get; set; }
            public string ClientId { get; set; } = "WUIAM";
            public DateTime CreatedAt { get; set; }
            public bool IsUsed { get; set; }
            public DateTime? UsedAt { get; set; }
            public bool IsExpired => ExpiresOn <= DateTime.UtcNow;
            public User? User { get; set; }


        public MFAToken()
        {
            CreatedAt = DateTime.UtcNow;
        } 
    }
}
