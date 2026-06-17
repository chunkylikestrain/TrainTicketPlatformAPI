namespace TrainTicketPlatformAPI.Models
{
    public class StateRegion
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public Country Country { get; set; } = null!;

        public ICollection<Locality> Localities { get; set; }
            = new List<Locality>();

        public ICollection<Station> Stations { get; set; }
            = new List<Station>();
    }
}
