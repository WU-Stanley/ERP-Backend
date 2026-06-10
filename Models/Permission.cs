namespace WUIAM.Models
{
    public class Permission
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!; // e.g. "user:create"
        public string? Description { get; set; }

        public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

}
