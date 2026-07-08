using System.Collections.Generic;

namespace WUIAM.DTOs
{
    public class HrSummaryReportDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public List<DepartmentHeadcountDto> HeadcountByDepartment { get; set; } = new();
        public List<EmploymentTypeBreakdownDto> HeadcountByEmploymentType { get; set; } = new();
        public List<GenderBreakdownDto> HeadcountByGender { get; set; } = new();
    }

    public class DepartmentHeadcountDto
    {
        public string DepartmentName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class EmploymentTypeBreakdownDto
    {
        public string EmploymentTypeName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class GenderBreakdownDto
    {
        public string Gender { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
