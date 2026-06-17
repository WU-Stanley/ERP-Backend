namespace WUIAM.DTOs
{
    public class SessionInfo
    {
        public int TokenId { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceName { get; set; }
        public string? Browser { get; set; }
        public string? IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsActive { get; set; }
    }
}
