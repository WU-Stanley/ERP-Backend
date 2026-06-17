namespace WUIAM.DTOs
{
    public class DepartmentResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DepartmentType { get; set; } = string.Empty;
        public Guid? CollegeId { get; set; }
        public string? CollegeName { get; set; }
        public Guid? ParentDepartmentId { get; set; }
        public string? ParentDepartmentName { get; set; }
        public Guid? HeadId { get; set; }
        public string? HeadName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
