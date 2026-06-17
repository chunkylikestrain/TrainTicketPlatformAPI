namespace TrainTicketPlatformAPI.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public ICollection<StateRegion> StateRegions { get; set; }
            = new List<StateRegion>();

        public ICollection<Station> Stations { get; set; }
            = new List<Station>();
    }
}
