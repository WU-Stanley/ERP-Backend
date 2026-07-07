using System;

namespace WUIAM.DTOs
{
    public class EndEmploymentDto
    {
        public DateTime ExitDate { get; set; } = DateTime.UtcNow;
        public string ReasonForLeaving { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}
