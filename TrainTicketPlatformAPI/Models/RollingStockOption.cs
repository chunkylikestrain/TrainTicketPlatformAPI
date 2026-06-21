namespace TrainTicketPlatformAPI.Models
{
    public class RollingStockOption
    {
        public int Id { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Series { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Manufacturer { get; set; } = string.Empty;
        public string MaxSpeed { get; set; } = string.Empty;
        public int? FleetCount { get; set; }
        public int? UnitCount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string Status { get; set; } = "Active";
    }
}
