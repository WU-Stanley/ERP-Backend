using System.Collections.Generic;

namespace WUIAM.Models
{
    public class UserType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation property to link to users
        public List<User> Users { get; set; } = new List<User>();
    }
 
}