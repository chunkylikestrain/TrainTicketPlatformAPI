namespace TrainTicketPlatformAPI.Contracts.Admin
{
    public class AdminAuditLogDto
    {
        public int Id { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public int? AdminUserId { get; set; }
        public string AdminEmail { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
    }
}
