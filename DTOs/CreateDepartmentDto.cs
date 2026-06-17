namespace WUIAM.DTOs
{
    public class CreateDepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public string? HeadOfDepartmentId { get; set; }
    }
}
