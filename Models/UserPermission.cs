namespace WUIAM.Models
{
    public class UserPermission
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = default!;

        public Guid PermissionId { get; set; }
        public Permission Permission { get; set; } = default!;

        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }
}
