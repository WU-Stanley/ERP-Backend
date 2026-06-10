namespace WUIAM.DTOs
{
    public class CreateDepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Guid HeadOfDepartmentId { get; set; }
    }
}
