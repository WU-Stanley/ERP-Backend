namespace WUIAM.DTOs
{
    public class AuditLogQueryDto
    {
        public Guid Id { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public Guid? UserId { get; set; }
        public string? UserName { get; set; }
        public string? EntityName { get; set; }
        public Guid? EntityId { get; set; }
        public string? Description { get; set; }
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AuditLogPaginationDto
    {
        public IEnumerable<AuditLogQueryDto> Logs { get; set; } = new List<AuditLogQueryDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }

    public class AuditLogStatsDto
    {
        public int TotalLogs { get; set; }
        public int LoginEvents { get; set; }
        public int LogoutEvents { get; set; }
        public int CreateEvents { get; set; }
        public int UpdateEvents { get; set; }
        public int DeleteEvents { get; set; }
        public IEnumerable<AuditLogEntityStatsDto> TopEntities { get; set; } = new List<AuditLogEntityStatsDto>();
    }

    public class AuditLogEntityStatsDto
    {
        public string EntityName { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
