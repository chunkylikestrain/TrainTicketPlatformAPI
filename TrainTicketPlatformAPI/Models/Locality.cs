namespace TrainTicketPlatformAPI.Models
{
    public class Locality
    {
        public int Id { get; set; }
        public int StateRegionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "City";

        public StateRegion StateRegion { get; set; } = null!;

        public ICollection<Station> Stations { get; set; }
            = new List<Station>();
    }
}
