using System;

namespace WUIAM.DTOs
{
    public class DepartmentSummaryDto
    {
        public Guid DepartmentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string DepartmentType { get; set; } = string.Empty;
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
    }
}
