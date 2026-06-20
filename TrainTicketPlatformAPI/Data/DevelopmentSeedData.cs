using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Data
{
    public static class DevelopmentSeedData
    {
        public const string AdminEmail = "admin@trainticket.dev";
        public const string PassengerEmail = "passenger@trainticket.dev";
        public const string DefaultPassword = "Password123!";
        public static readonly DateTime MainTripDepartureUtc = new(2026, 7, 1, 8, 0, 0, DateTimeKind.Utc);

        private sealed record CountrySeed(string Code, string Name);
        private sealed record RegionSeed(string CountryCode, string Code, string Name);
        private sealed record StationSeed(
            string CountryCode,
            string RegionCode,
            string LocalityName,
            string LocalityType,
            string StationCode,
            string StationName,
            string City);
        private sealed record DemoRouteSeed(
            string Code,
            string OriginCode,
            string DestinationCode,
            decimal DistanceKm,
            int DurationMinutes,
            string[] StopCodes);
        private sealed record DemoTrainSeed(
            string Code,
            string Name,
            string Type,
            int CarriageCount,
            int SeatsPerCarriage);
        private sealed record DemoScheduleSeed(
            string TrainCode,
            string RouteCode,
            TimeSpan DepartureTime,
            int DurationMinutes,
            decimal Class1Price,
            decimal Class2Price,
            string Platform,
            string Track);
        private sealed record StationDisplaySeed(string Code, string Name, string City, string LocalityName);

        private static readonly CountrySeed[] ReferenceCountries =
        [
            new("PL", "Poland"),
            new("DE", "Germany"),
            new("CZ", "Czech Republic"),
            new("SK", "Slovakia"),
            new("LT", "Lithuania"),
            new("UA", "Ukraine"),
            new("AT", "Austria")
        ];

        private static readonly RegionSeed[] ReferenceRegions =
        [
            new("PL", "DS", "Dolnoslaskie"),
            new("PL", "KP", "Kujawsko-Pomorskie"),
            new("PL", "LB", "Lubelskie"),
            new("PL", "LU", "Lubuskie"),
            new("PL", "LD", "Lodzkie"),
            new("PL", "MA", "Malopolskie"),
            new("PL", "MZ", "Mazowieckie"),
            new("PL", "OP", "Opolskie"),
            new("PL", "PK", "Podkarpackie"),
            new("PL", "PD", "Podlaskie"),
            new("PL", "PM", "Pomorskie"),
            new("PL", "SL", "Slaskie"),
            new("PL", "SK", "Swietokrzyskie"),
            new("PL", "WM", "Warminsko-Mazurskie"),
            new("PL", "WP", "Wielkopolskie"),
            new("PL", "ZP", "Zachodniopomorskie"),
            new("DE", "BE", "Berlin"),
            new("DE", "BB", "Brandenburg"),
            new("CZ", "PR", "Prague"),
            new("CZ", "MS", "Moravian-Silesian"),
            new("SK", "BA", "Bratislava"),
            new("LT", "VL", "Vilnius"),
            new("UA", "KV", "Kyiv"),
            new("UA", "LV", "Lviv"),
            new("AT", "WI", "Vienna")
        ];

        private static readonly StationSeed[] ReferenceStations =
        [
            new("PL", "WP", "Poznań", "City", "POZ", "Poznań Główny", "Poznań"),
            new("PL", "MA", "Kraków", "City", "KRK", "Kraków Główny", "Kraków"),
            new("PL", "MZ", "Warszawa", "City", "WAW", "Warszawa Centralna", "Warszawa"),
            new("PL", "DS", "Wrocław", "City", "WRO", "Wrocław Główny", "Wrocław"),
            new("PL", "MZ", "Warszawa", "City", "WAWZ", "Warszawa Zachodnia", "Warszawa"),
            new("PL", "MZ", "Warszawa", "City", "WAWW", "Warszawa Wschodnia", "Warszawa"),
            new("PL", "PM", "Gdańsk", "City", "GDN", "Gdańsk Główny", "Gdańsk"),
            new("PL", "ZP", "Szczecin", "City", "SZZ", "Szczecin Główny", "Szczecin"),
            new("PL", "SL", "Katowice", "City", "KTW", "Katowice", "Katowice"),
            new("PL", "LB", "Lublin", "City", "LUB", "Lublin Główny", "Lublin"),
            new("PL", "PM", "Gdynia", "City", "GDY", "Gdynia Główna", "Gdynia"),
            new("PL", "PD", "Białystok", "City", "BIA", "Białystok", "Białystok"),
            new("PL", "KP", "Bydgoszcz", "City", "BYD", "Bydgoszcz Główna", "Bydgoszcz"),
            new("PL", "WM", "Olsztyn", "City", "OLS", "Olsztyn Główny", "Olsztyn"),
            new("PL", "PK", "Rzeszów", "City", "RZE", "Rzeszów Główny", "Rzeszów"),
            new("PL", "OP", "Opole", "City", "OPO", "Opole Główne", "Opole"),
            new("PL", "MA", "Zakopane", "Town", "ZAK", "Zakopane", "Zakopane"),
            new("PL", "LU", "Zielona Góra", "City", "ZGR", "Zielona Góra Główna", "Zielona Góra"),
            new("PL", "KP", "Bydgoszcz", "City", "RYW", "Rynkowo Wiadukt", "Bydgoszcz"),
            new("PL", "MZ", "Radom", "City", "RAD", "Radom Główny", "Radom"),
            new("PL", "LB", "Puławy", "Town", "PUM", "Puławy Miasto", "Puławy"),
            new("PL", "MZ", "Warszawa", "City", "WAWG", "Warszawa Gdańska", "Warszawa"),
            new("PL", "LD", "Łódź", "City", "LOD", "Łódź Fabryczna", "Łódź"),
            new("PL", "MZ", "Otwock", "Town", "OTW", "Otwock", "Otwock"),
            new("PL", "KP", "Toruń", "City", "TOR", "Toruń Główny", "Toruń"),
            new("PL", "PK", "Przemyśl", "City", "PRZ", "Przemyśl Główny", "Przemyśl"),
            new("PL", "SK", "Kielce", "City", "KIE", "Kielce Główne", "Kielce"),
            new("PL", "MZ", "Warszawa", "City", "WAWO", "Warszawa Ochota", "Warszawa"),
            new("PL", "MZ", "Warszawa", "City", "WAWSL", "Warszawa Służewiec", "Warszawa"),
            new("PL", "MZ", "Mińsk Mazowiecki", "Town", "MIM", "Mińsk Mazowiecki", "Mińsk Mazowiecki"),
            new("PL", "MZ", "Siedlce", "City", "SIE", "Siedlce", "Siedlce"),
            new("PL", "SL", "Częstochowa", "City", "CZE", "Częstochowa", "Częstochowa"),
            new("PL", "DS", "Legnica", "City", "LEG", "Legnica", "Legnica"),
            new("PL", "MZ", "Pilawa", "Town", "PILW", "Pilawa", "Pilawa"),
            new("PL", "MA", "Tarnów", "City", "TAR", "Tarnów", "Tarnów"),
            new("PL", "ZP", "Świnoujście", "City", "SWI", "Świnoujście", "Świnoujście"),
            new("PL", "ZP", "Koszalin", "City", "KOS", "Koszalin", "Koszalin"),
            new("PL", "LD", "Skierniewice", "City", "SKI", "Skierniewice", "Skierniewice"),
            new("PL", "PM", "Gdańsk", "City", "GDW", "Gdańsk Wrzeszcz", "Gdańsk"),
            new("PL", "LD", "Kutno", "Town", "KUT", "Kutno", "Kutno"),
            new("PL", "PM", "Lębork", "Town", "LEB", "Lębork", "Lębork"),
            new("PL", "PM", "Tczew", "Town", "TCZ", "Tczew", "Tczew"),
            new("PL", "MZ", "Żyrardów", "Town", "ZYR", "Żyrardów", "Żyrardów"),
            new("PL", "LD", "Łódź", "City", "LODK", "Łódź Kaliska", "Łódź"),
            new("PL", "MZ", "Legionowo", "Town", "LGO", "Legionowo", "Legionowo"),
            new("PL", "WP", "Gniezno", "City", "GNZ", "Gniezno", "Gniezno"),
            new("PL", "MZ", "Warszawa", "City", "WAST", "Warszawa Stadion", "Warszawa"),
            new("PL", "PM", "Wejherowo", "Town", "WEJ", "Wejherowo", "Wejherowo"),
            new("PL", "MZ", "Warszawa", "City", "WAPW", "Warszawa Powiśle", "Warszawa"),
            new("PL", "MZ", "Piaseczno", "Town", "PIA", "Piaseczno", "Piaseczno"),
            new("PL", "ZP", "Stargard", "City", "STG", "Stargard", "Stargard"),
            new("PL", "ZP", "Szczecinek", "Town", "SZK", "Szczecinek", "Szczecinek"),
            new("PL", "WP", "Oborniki", "Town", "OBM", "Oborniki Wielkopolskie Miasto", "Oborniki"),
            new("PL", "PM", "Słupsk", "City", "SLU", "Słupsk", "Słupsk"),
            new("PL", "MZ", "Grodzisk Mazowiecki", "Town", "GDM", "Grodzisk Mazowiecki", "Grodzisk Mazowiecki"),
            new("PL", "LB", "Chełm", "City", "CHL", "Chełm", "Chełm"),
            new("PL", "MZ", "Sochaczew", "Town", "SOC", "Sochaczew", "Sochaczew"),
            new("PL", "LU", "Kostrzyn nad Odrą", "Town", "KNO", "Kostrzyn", "Kostrzyn nad Odrą"),
            new("PL", "MZ", "Ożarów Mazowiecki", "Town", "OZM", "Ożarów Mazowiecki", "Ożarów Mazowiecki"),
            new("PL", "DS", "Jelenia Góra", "City", "JEG", "Jelenia Góra", "Jelenia Góra"),
            new("PL", "SL", "Rybnik", "City", "RYB", "Rybnik", "Rybnik"),
            new("PL", "SL", "Zawiercie", "Town", "ZAW", "Zawiercie", "Zawiercie"),
            new("PL", "WP", "Ostrów Wielkopolski", "City", "OSTW", "Ostrów Wielkopolski", "Ostrów Wielkopolski"),
            new("PL", "PM", "Sopot", "City", "SOP", "Sopot", "Sopot"),
            new("PL", "MZ", "Ciechanów", "City", "CIE", "Ciechanów", "Ciechanów"),
            new("PL", "OP", "Kędzierzyn-Koźle", "Town", "KKO", "Kędzierzyn-Koźle", "Kędzierzyn-Koźle"),
            new("PL", "WP", "Konin", "City", "KON", "Konin", "Konin"),
            new("PL", "WP", "Kościan", "Town", "KSC", "Kościan", "Kościan"),
            new("PL", "ZP", "Białogard", "Town", "BLG", "Białogard", "Białogard"),
            new("PL", "MZ", "Warszawa", "City", "WREM", "Warszawa Rembertów", "Warszawa"),
            new("PL", "LU", "Zbąszynek", "Town", "ZBA", "Zbąszynek", "Zbąszynek"),
            new("PL", "WM", "Iława", "Town", "ILA", "Iława Główna", "Iława"),
            new("PL", "DS", "Oława", "Town", "OLA", "Oława", "Oława"),
            new("PL", "PK", "Dębica", "Town", "DEB", "Dębica", "Dębica"),
            new("PL", "PM", "Malbork", "Town", "MAL", "Malbork", "Malbork"),
            new("PL", "LB", "Łuków", "Town", "LUK", "Łuków", "Łuków"),
            new("PL", "MZ", "Warka", "Town", "WAR", "Warka", "Warka"),
            new("PL", "SL", "Czechowice-Dziedzice", "Town", "CZD", "Czechowice-Dziedzice", "Czechowice-Dziedzice"),
            new("PL", "SL", "Tychy", "City", "TYC", "Tychy", "Tychy"),
            new("PL", "PM", "Chojnice", "Town", "CHO", "Chojnice", "Chojnice"),
            new("PL", "MA", "Kraków", "City", "KRP", "Kraków Płaszów", "Kraków"),
            new("PL", "MA", "Bochnia", "Town", "BOC", "Bochnia", "Bochnia"),
            new("PL", "DS", "Lubin", "Town", "LBN", "Lubin", "Lubin"),
            new("PL", "MZ", "Ostrołęka", "City", "OSL", "Ostrołęka", "Ostrołęka"),
            new("PL", "LD", "Piotrków Trybunalski", "City", "PIT", "Piotrków Trybunalski", "Piotrków Trybunalski"),
            new("PL", "MA", "Krzeszowice", "Town", "KRZ", "Krzeszowice", "Krzeszowice"),
            new("PL", "WP", "Kalisz", "City", "KAL", "Kalisz", "Kalisz"),
            new("PL", "MZ", "Warszawa", "City", "WAWI", "Warszawa Wileńska", "Warszawa"),
            new("PL", "SL", "Gliwice", "City", "GLI", "Gliwice", "Gliwice"),
            new("PL", "SL", "Bielsko-Biała", "City", "BBI", "Bielsko-Biała Główna", "Bielsko-Biała"),
            new("PL", "ZP", "Kołobrzeg", "Town", "KOL", "Kołobrzeg", "Kołobrzeg"),
            new("PL", "KP", "Włocławek", "City", "WLO", "Włocławek", "Włocławek"),
            new("PL", "KP", "Inowrocław", "City", "INO", "Inowrocław", "Inowrocław"),
            new("PL", "WP", "Piła", "City", "PILA", "Piła Główna", "Piła"),
            new("PL", "WP", "Leszno", "City", "LES", "Leszno", "Leszno"),
            new("PL", "LB", "Zamość", "City", "ZAM", "Zamość", "Zamość"),
            new("PL", "PD", "Suwałki", "City", "SUW", "Suwałki", "Suwałki"),
            new("PL", "WM", "Elbląg", "City", "ELB", "Elbląg", "Elbląg"),
            new("PL", "DS", "Wałbrzych", "City", "WLB", "Wałbrzych Główny", "Wałbrzych"),
            new("DE", "BE", "Berlin", "City", "BERHBF", "Berlin Hauptbahnhof", "Berlin"),
            new("DE", "BB", "Frankfurt (Oder)", "City", "FFO", "Frankfurt (Oder)", "Frankfurt (Oder)"),
            new("CZ", "PR", "Prague", "City", "PRG", "Praha hlavní nádraží", "Prague"),
            new("CZ", "MS", "Ostrava", "City", "OST", "Ostrava-Svinov", "Ostrava"),
            new("CZ", "MS", "Bohumín", "Town", "BOH", "Bohumín", "Bohumín"),
            new("SK", "BA", "Bratislava", "City", "BTS", "Bratislava hlavná stanica", "Bratislava"),
            new("LT", "VL", "Vilnius", "City", "VNO", "Vilnius", "Vilnius"),
            new("UA", "KV", "Kyiv", "City", "KYV", "Kyiv-Pasazhyrskyi", "Kyiv"),
            new("UA", "LV", "Lviv", "City", "LVI", "Lviv", "Lviv"),
            new("AT", "WI", "Vienna", "City", "VIE", "Wien Hauptbahnhof", "Vienna")
        ];

        private static readonly DemoRouteSeed[] DemoRoutes =
        [
            new("KRK-GDY", "KRK", "GDY", 600m, 360, ["WAWZ", "WAW", "WAWW", "ILA", "MAL", "TCZ", "GDN", "GDW", "SOP"]),
            new("KRP-GDY", "KRP", "GDY", 650m, 360, ["KRK", "WAWZ", "WAW", "WAWW", "ILA", "MAL", "TCZ", "GDN", "GDW", "SOP"]),
            new("WAW-KRK", "WAW", "KRK", 293m, 160, ["WAWZ", "KRZ"]),
            new("KRK-WAW", "KRK", "WAW", 293m, 160, ["KRZ", "WAWZ"]),
            new("WAW-GDN", "WAW", "GDN", 340m, 175, ["WAWW", "ILA", "MAL", "TCZ"]),
            new("GDN-WAW", "GDN", "WAW", 340m, 175, ["TCZ", "MAL", "ILA", "WAWW"]),
            new("RZE-WAW", "RZE", "WAW", 330m, 250, ["TAR", "KRK", "KRZ", "WAWZ"]),
            new("WAW-RZE", "WAW", "RZE", 330m, 250, ["WAWZ", "KRZ", "KRK", "TAR"]),
            new("WRO-WAW", "WRO", "WAW", 350m, 245, ["OPO", "CZE", "WAWZ"]),
            new("WAW-WRO", "WAW", "WRO", 350m, 245, ["WAWZ", "CZE", "OPO"]),
            new("POZ-WAW", "POZ", "WAW", 305m, 170, ["KON", "KUT", "WAWZ"]),
            new("WAW-POZ", "WAW", "POZ", 305m, 170, ["WAWZ", "KUT", "KON"]),
            new("SZZ-WAW", "SZZ", "WAW", 565m, 330, ["STG", "POZ", "KON", "KUT", "WAWZ"]),
            new("PRZ-KOL", "PRZ", "KOL", 950m, 710, ["RZE", "TAR", "KRK", "KRP", "KTW", "GLI", "OPO", "WRO", "POZ", "PILA", "KOS"])
        ];

        private static readonly DemoTrainSeed[] DemoTrains =
        [
            new("EIP-3508", "EIP 3508", "Express InterCity Premium", 6, 48),
            new("EIP-3510", "EIP 3510", "Express InterCity Premium", 6, 48),
            new("EIC-1602", "EIC 1602 Kaszub", "Express InterCity", 5, 52),
            new("IC-56", "IC 56 Wawel", "InterCity", 5, 56),
            new("IC-3806", "IC 3806 Zefir", "InterCity", 4, 56),
            new("IC-6102", "IC 6102 Heweliusz", "InterCity", 5, 56),
            new("IC-7310", "IC 7310 Malczewski", "InterCity", 4, 56),
            new("IC-8120", "IC 8120 Odra", "InterCity", 4, 56),
            new("TLK-38170", "TLK 38170 Ustronie", "Twoje Linie Kolejowe", 6, 60)
        ];

        private static readonly DemoScheduleSeed[] DemoSchedules =
        [
            new("EIP-3508", "KRK-GDY", new TimeSpan(6, 6, 0), 360, 134m, 90m, "3", "1"),
            new("IC-56", "KRK-GDY", new TimeSpan(6, 19, 0), 381, 62m, 47m, "4", "2"),
            new("IC-3806", "KRP-GDY", new TimeSpan(6, 54, 0), 393, 62m, 47m, "4", "1"),
            new("EIP-3510", "WAW-KRK", new TimeSpan(7, 12, 0), 160, 119m, 78m, "2", "3"),
            new("IC-56", "KRK-WAW", new TimeSpan(15, 10, 0), 170, 84m, 59m, "2", "2"),
            new("EIC-1602", "WAW-GDN", new TimeSpan(8, 25, 0), 175, 112m, 76m, "5", "1"),
            new("IC-6102", "GDN-WAW", new TimeSpan(16, 40, 0), 185, 98m, 65m, "2", "4"),
            new("IC-7310", "RZE-WAW", new TimeSpan(5, 48, 0), 250, 92m, 64m, "1", "2"),
            new("IC-7310", "WAW-RZE", new TimeSpan(17, 35, 0), 250, 92m, 64m, "6", "1"),
            new("IC-8120", "WRO-WAW", new TimeSpan(9, 5, 0), 245, 99m, 69m, "3", "5"),
            new("IC-8120", "WAW-WRO", new TimeSpan(18, 15, 0), 245, 99m, 69m, "7", "2"),
            new("EIC-1602", "POZ-WAW", new TimeSpan(6, 45, 0), 170, 89m, 57m, "4", "4"),
            new("EIC-1602", "WAW-POZ", new TimeSpan(14, 20, 0), 170, 89m, 57m, "8", "1"),
            new("TLK-38170", "SZZ-WAW", new TimeSpan(7, 30, 0), 330, 72m, 44m, "1", "1"),
            new("TLK-38170", "PRZ-KOL", new TimeSpan(22, 18, 0), 710, 115m, 82m, "2", "6")
        ];

        private static readonly StationDisplaySeed[] CleanStationDisplays =
        [
            new("POZ", "Poznan Glowny", "Poznan", "Poznan"),
            new("KRK", "Krakow Glowny", "Krakow", "Krakow"),
            new("KRP", "Krakow Plaszow", "Krakow", "Krakow"),
            new("WAW", "Warszawa Centralna", "Warszawa", "Warszawa"),
            new("WAWZ", "Warszawa Zachodnia", "Warszawa", "Warszawa"),
            new("WAWW", "Warszawa Wschodnia", "Warszawa", "Warszawa"),
            new("WRO", "Wroclaw Glowny", "Wroclaw", "Wroclaw"),
            new("GDN", "Gdansk Glowny", "Gdansk", "Gdansk"),
            new("GDW", "Gdansk Wrzeszcz", "Gdansk", "Gdansk"),
            new("GDY", "Gdynia Glowna", "Gdynia", "Gdynia"),
            new("RZE", "Rzeszow Glowny", "Rzeszow", "Rzeszow"),
            new("SZZ", "Szczecin Glowny", "Szczecin", "Szczecin"),
            new("PRZ", "Przemysl Glowny", "Przemysl", "Przemysl"),
            new("KOL", "Kolobrzeg", "Kolobrzeg", "Kolobrzeg"),
            new("ILA", "Ilawa Glowna", "Ilawa", "Ilawa"),
            new("MAL", "Malbork", "Malbork", "Malbork"),
            new("TCZ", "Tczew", "Tczew", "Tczew"),
            new("SOP", "Sopot", "Sopot", "Sopot"),
            new("TAR", "Tarnow", "Tarnow", "Tarnow"),
            new("KRZ", "Krzeszowice", "Krzeszowice", "Krzeszowice"),
            new("KTW", "Katowice", "Katowice", "Katowice"),
            new("GLI", "Gliwice", "Gliwice", "Gliwice"),
            new("OPO", "Opole Glowne", "Opole", "Opole"),
            new("CZE", "Czestochowa", "Czestochowa", "Czestochowa"),
            new("KON", "Konin", "Konin", "Konin"),
            new("KUT", "Kutno", "Kutno", "Kutno"),
            new("STG", "Stargard", "Stargard", "Stargard"),
            new("PILA", "Pila Glowna", "Pila", "Pila"),
            new("KOS", "Koszalin", "Koszalin", "Koszalin")
        ];

        public static async Task SeedAsync(
            TrainTicketDbContext db,
            IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            if (!configuration.GetValue("SeedData:UseDevelopmentSeedData", true))
                return;

            var seedLocations = await EnsureReferenceLocationsAsync(db, cancellationToken);
            await EnsureCleanStationDisplaysAsync(db, cancellationToken);

            var country = seedLocations.Countries["PL"];
            var mazowieckie = seedLocations.Regions[RegionKey("PL", "MZ")];
            var malopolskie = seedLocations.Regions[RegionKey("PL", "MA")];
            var pomorskie = seedLocations.Regions[RegionKey("PL", "PM")];

            var warsaw = await EnsureLocalityAsync(db, mazowieckie, "Warsaw", "City", cancellationToken);
            var krakow = await EnsureLocalityAsync(db, malopolskie, "Krakow", "City", cancellationToken);
            var gdansk = await EnsureLocalityAsync(db, pomorskie, "Gdansk", "City", cancellationToken);

            var waw = await EnsureStationAsync(db, country, mazowieckie, warsaw, "WAW", "Warszawa Centralna", "Warsaw", cancellationToken);
            var krk = await EnsureStationAsync(db, country, malopolskie, krakow, "KRK", "Krakow Glowny", "Krakow", cancellationToken);
            var gdn = await EnsureStationAsync(db, country, pomorskie, gdansk, "GDN", "Gdansk Glowny", "Gdansk", cancellationToken);

            var wawKrk = await EnsureRouteAsync(db, waw, krk, 293m, cancellationToken);
            var krkGdn = await EnsureRouteAsync(db, krk, gdn, 600m, cancellationToken);

            var ic101 = await EnsureTrainAsync(db, "IC 101", "Warsaw", "Krakow", MainTripDepartureUtc, MainTripDepartureUtc.AddHours(3), cancellationToken);
            var ic202 = await EnsureTrainAsync(db, "IC 202", "Krakow", "Gdansk", MainTripDepartureUtc.AddHours(2), MainTripDepartureUtc.AddHours(8), cancellationToken);

            var mainTrip = await EnsureTripAsync(db, ic101, wawKrk, MainTripDepartureUtc, MainTripDepartureUtc.AddHours(3), cancellationToken);
            var secondTrip = await EnsureTripAsync(db, ic202, krkGdn, MainTripDepartureUtc.AddHours(2), MainTripDepartureUtc.AddHours(8), cancellationToken);

            await EnsureFaresAsync(db, mainTrip, cancellationToken);
            await EnsureFaresAsync(db, secondTrip, cancellationToken);
            await EnsureSeatsAsync(db, ic101, cancellationToken);
            await EnsureSeatsAsync(db, ic202, cancellationToken);
            await EnsureDemoSchedulesAsync(db, cancellationToken);

            await EnsureUserAsync(db, configuration, "SeedData:AdminPassword", AdminEmail, "Admin", cancellationToken);
            await EnsureUserAsync(db, configuration, "SeedData:PassengerPassword", PassengerEmail, "Passenger", cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task<SeedLocationIndex> EnsureReferenceLocationsAsync(
            TrainTicketDbContext db,
            CancellationToken cancellationToken)
        {
            var countries = new Dictionary<string, Country>(StringComparer.OrdinalIgnoreCase);
            foreach (var seed in ReferenceCountries)
            {
                countries[seed.Code] = await EnsureCountryAsync(db, seed.Code, seed.Name, cancellationToken);
            }

            var regions = new Dictionary<string, StateRegion>(StringComparer.OrdinalIgnoreCase);
            foreach (var seed in ReferenceRegions)
            {
                var country = countries[seed.CountryCode];
                regions[RegionKey(seed.CountryCode, seed.Code)] = await EnsureStateRegionAsync(
                    db,
                    country,
                    seed.Code,
                    seed.Name,
                    cancellationToken);
            }

            foreach (var seed in ReferenceStations)
            {
                var country = countries[seed.CountryCode];
                var region = regions[RegionKey(seed.CountryCode, seed.RegionCode)];
                var locality = await EnsureLocalityAsync(
                    db,
                    region,
                    seed.LocalityName,
                    seed.LocalityType,
                    cancellationToken);

                await EnsureStationAsync(
                    db,
                    country,
                    region,
                    locality,
                    seed.StationCode,
                    seed.StationName,
                    seed.City,
                    cancellationToken);
            }

            return new SeedLocationIndex(countries, regions);
        }

        private static async Task EnsureCleanStationDisplaysAsync(
            TrainTicketDbContext db,
            CancellationToken cancellationToken)
        {
            foreach (var seed in CleanStationDisplays)
            {
                var station = await db.Stations
                    .Include(s => s.Locality)
                    .FirstOrDefaultAsync(s => s.Code == seed.Code, cancellationToken);

                if (station == null)
                    continue;

                station.Name = seed.Name;
                station.City = seed.City;

                if (station.Locality != null)
                    station.Locality.Name = seed.LocalityName;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private sealed record SeedLocationIndex(
            Dictionary<string, Country> Countries,
            Dictionary<string, StateRegion> Regions);

        private static string RegionKey(string countryCode, string regionCode)
            => $"{countryCode}:{regionCode}";

        private static async Task<Country> EnsureCountryAsync(TrainTicketDbContext db, CancellationToken cancellationToken)
            => await EnsureCountryAsync(db, "PL", "Poland", cancellationToken);

        private static async Task<Country> EnsureCountryAsync(
            TrainTicketDbContext db,
            string code,
            string name,
            CancellationToken cancellationToken)
        {
            var country = await db.Countries.FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
            if (country != null)
            {
                if (country.Name != name)
                    country.Name = name;

                return country;
            }

            country = new Country { Code = code, Name = name };
            db.Countries.Add(country);
            await db.SaveChangesAsync(cancellationToken);
            return country;
        }

        private static async Task<StateRegion> EnsureStateRegionAsync(
            TrainTicketDbContext db,
            Country country,
            string code,
            string name,
            CancellationToken cancellationToken)
        {
            var region = await db.StateRegions
                .FirstOrDefaultAsync(r => r.CountryId == country.Id && r.Code == code, cancellationToken);
            if (region != null)
            {
                if (region.Name != name)
                    region.Name = name;

                return region;
            }

            region = new StateRegion { CountryId = country.Id, Code = code, Name = name };
            db.StateRegions.Add(region);
            await db.SaveChangesAsync(cancellationToken);
            return region;
        }

        private static async Task<Locality> EnsureLocalityAsync(
            TrainTicketDbContext db,
            StateRegion region,
            string name,
            string type,
            CancellationToken cancellationToken)
        {
            var locality = await db.Localities
                .FirstOrDefaultAsync(l =>
                    l.StateRegionId == region.Id &&
                    l.Name == name &&
                    l.Type == type,
                    cancellationToken);
            if (locality != null)
            {
                if (locality.Name != name || locality.Type != type)
                {
                    locality.Name = name;
                    locality.Type = type;
                }

                return locality;
            }

            locality = new Locality { StateRegionId = region.Id, Name = name, Type = type };
            db.Localities.Add(locality);
            await db.SaveChangesAsync(cancellationToken);
            return locality;
        }

        private static async Task<Station> EnsureStationAsync(
            TrainTicketDbContext db,
            Country country,
            StateRegion region,
            Locality locality,
            string code,
            string name,
            string city,
            CancellationToken cancellationToken)
        {
            var station = await db.Stations.FirstOrDefaultAsync(s => s.Code == code, cancellationToken);
            if (station != null)
            {
                station.Name = name;
                station.City = city;
                station.CountryId = country.Id;
                station.StateRegionId = region.Id;
                station.LocalityId = locality.Id;
                return station;
            }

            station = new Station
            {
                Code = code,
                Name = name,
                City = city,
                CountryId = country.Id,
                StateRegionId = region.Id,
                LocalityId = locality.Id
            };
            db.Stations.Add(station);
            await db.SaveChangesAsync(cancellationToken);
            return station;
        }

        private static async Task<TrainRoute> EnsureRouteAsync(
            TrainTicketDbContext db,
            Station departure,
            Station arrival,
            decimal distanceKm,
            CancellationToken cancellationToken)
        {
            var route = await db.TrainRoutes
                .FirstOrDefaultAsync(r =>
                    r.DepartureStationId == departure.Id &&
                    r.ArrivalStationId == arrival.Id,
                    cancellationToken);
            if (route != null)
                return route;

            route = new TrainRoute
            {
                DepartureStationId = departure.Id,
                ArrivalStationId = arrival.Id,
                DistanceKm = distanceKm,
                IsActive = true
            };
            db.TrainRoutes.Add(route);
            await db.SaveChangesAsync(cancellationToken);
            return route;
        }

        private static async Task<Train> EnsureTrainAsync(
            TrainTicketDbContext db,
            string name,
            string departureStation,
            string arrivalStation,
            DateTime departureTime,
            DateTime arrivalTime,
            CancellationToken cancellationToken)
        {
            var train = await db.Trains.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
            if (train != null)
                return train;

            train = new Train
            {
                Name = name,
                DepartureStation = departureStation,
                ArrivalStation = arrivalStation,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime
            };
            db.Trains.Add(train);
            await db.SaveChangesAsync(cancellationToken);
            return train;
        }

        private static async Task<Trip> EnsureTripAsync(
            TrainTicketDbContext db,
            Train train,
            TrainRoute route,
            DateTime departure,
            DateTime arrival,
            CancellationToken cancellationToken)
        {
            var trip = await db.Trips
                .FirstOrDefaultAsync(t =>
                    t.TrainId == train.Id &&
                    t.TrainRouteId == route.Id &&
                    t.DepartureTime == departure,
                    cancellationToken);
            if (trip != null)
                return trip;

            trip = new Trip
            {
                TrainId = train.Id,
                TrainRouteId = route.Id,
                DepartureTime = departure,
                ArrivalTime = arrival,
                Status = "Scheduled"
            };
            db.Trips.Add(trip);
            await db.SaveChangesAsync(cancellationToken);
            return trip;
        }

        private static async Task EnsureFaresAsync(TrainTicketDbContext db, Trip trip, CancellationToken cancellationToken)
        {
            if (await db.Fares.AnyAsync(f => f.TripId == trip.Id, cancellationToken))
                return;

            db.Fares.AddRange(
                new Fare { TripId = trip.Id, ClassType = "Economy", Price = 49.99m, Currency = "USD" },
                new Fare { TripId = trip.Id, ClassType = "Business", Price = 89.99m, Currency = "USD" });
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureSeatsAsync(TrainTicketDbContext db, Train train, CancellationToken cancellationToken)
        {
            if (await db.Seats.AnyAsync(s => s.TrainId == train.Id, cancellationToken))
                return;

            var carriageCount = Math.Max(1, train.CarriageCount);
            var seatsPerCarriage = Math.Max(4, train.SeatsPerCarriage);
            var seats = new List<Seat>();

            for (var carriage = 1; carriage <= carriageCount; carriage++)
            {
                var classType = carriage == 1 ? "Class 1" : "Class 2";
                for (var number = 1; number <= seatsPerCarriage; number++)
                {
                    seats.Add(new Seat
                    {
                        TrainId = train.Id,
                        Coach = carriage.ToString(),
                        Number = number.ToString(),
                        ClassType = classType,
                        IsAvailable = true
                    });
                }
            }

            db.Seats.AddRange(seats);
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureDemoSchedulesAsync(
            TrainTicketDbContext db,
            CancellationToken cancellationToken)
        {
            var routes = new Dictionary<string, TrainRoute>(StringComparer.OrdinalIgnoreCase);
            foreach (var routeSeed in DemoRoutes)
            {
                routes[routeSeed.Code] = await EnsureDemoRouteAsync(db, routeSeed, cancellationToken);
            }

            var trains = new Dictionary<string, Train>(StringComparer.OrdinalIgnoreCase);
            foreach (var trainSeed in DemoTrains)
            {
                var train = await EnsureDemoTrainAsync(db, trainSeed, cancellationToken);
                await EnsureSeatsAsync(db, train, cancellationToken);
                trains[trainSeed.Code] = train;
            }

            var firstServiceDate = DateTime.Today.Date;
            const int serviceDays = 21;

            foreach (var scheduleSeed in DemoSchedules)
            {
                if (!trains.TryGetValue(scheduleSeed.TrainCode, out var train) ||
                    !routes.TryGetValue(scheduleSeed.RouteCode, out var route))
                {
                    continue;
                }

                for (var dayOffset = 0; dayOffset < serviceDays; dayOffset++)
                {
                    var departure = firstServiceDate.AddDays(dayOffset).Add(scheduleSeed.DepartureTime);
                    var arrival = departure.AddMinutes(scheduleSeed.DurationMinutes);
                    var trip = await EnsureTripAsync(db, train, route, departure, arrival, cancellationToken);

                    trip.Platform = scheduleSeed.Platform;
                    trip.Track = scheduleSeed.Track;
                    trip.Status = "Scheduled";

                    await EnsureFaresAsync(
                        db,
                        trip,
                        scheduleSeed.Class1Price,
                        scheduleSeed.Class2Price,
                        cancellationToken);
                }
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task<TrainRoute> EnsureDemoRouteAsync(
            TrainTicketDbContext db,
            DemoRouteSeed seed,
            CancellationToken cancellationToken)
        {
            var origin = await GetStationByCodeAsync(db, seed.OriginCode, cancellationToken);
            var destination = await GetStationByCodeAsync(db, seed.DestinationCode, cancellationToken);

            var route = await db.TrainRoutes
                .Include(r => r.RouteStops)
                .FirstOrDefaultAsync(r =>
                    r.DepartureStationId == origin.Id &&
                    r.ArrivalStationId == destination.Id,
                    cancellationToken);

            if (route == null)
            {
                route = new TrainRoute
                {
                    DepartureStationId = origin.Id,
                    ArrivalStationId = destination.Id,
                    IsActive = true
                };
                db.TrainRoutes.Add(route);
            }

            route.Code = seed.Code;
            route.Name = $"{origin.Name} to {destination.Name}";
            route.DistanceKm = seed.DistanceKm;
            route.EstimatedDurationMinutes = seed.DurationMinutes;
            route.OperatingDays = "Daily";
            route.IntermediateStops = string.Join(Environment.NewLine, seed.StopCodes);

            if (route.RouteStops.Count == 0)
            {
                for (var index = 0; index < seed.StopCodes.Length; index++)
                {
                    var station = await GetStationByCodeAsync(db, seed.StopCodes[index], cancellationToken);
                    route.RouteStops.Add(new TrainRouteStop
                    {
                        StationId = station.Id,
                        StopOrder = index + 1
                    });
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            return route;
        }

        private static async Task<Station> GetStationByCodeAsync(
            TrainTicketDbContext db,
            string code,
            CancellationToken cancellationToken)
        {
            return await db.Stations
                .FirstOrDefaultAsync(s => s.Code == code, cancellationToken)
                ?? throw new InvalidOperationException($"Seed station '{code}' was not found.");
        }

        private static async Task<Train> EnsureDemoTrainAsync(
            TrainTicketDbContext db,
            DemoTrainSeed seed,
            CancellationToken cancellationToken)
        {
            var train = await db.Trains
                .FirstOrDefaultAsync(t => t.Code == seed.Code || t.Name == seed.Name, cancellationToken);

            if (train == null)
            {
                train = new Train();
                db.Trains.Add(train);
            }

            train.Code = seed.Code;
            train.Name = seed.Name;
            train.Type = seed.Type;
            train.CarriageCount = seed.CarriageCount;
            train.SeatsPerCarriage = seed.SeatsPerCarriage;
            train.Status = "Active";

            await db.SaveChangesAsync(cancellationToken);
            return train;
        }

        private static async Task EnsureFaresAsync(
            TrainTicketDbContext db,
            Trip trip,
            decimal class1Price,
            decimal class2Price,
            CancellationToken cancellationToken)
        {
            await UpsertFareAsync(db, trip, "Class 1", class1Price, cancellationToken);
            await UpsertFareAsync(db, trip, "Class 2", class2Price, cancellationToken);
        }

        private static async Task UpsertFareAsync(
            TrainTicketDbContext db,
            Trip trip,
            string classType,
            decimal price,
            CancellationToken cancellationToken)
        {
            var fare = await db.Fares
                .FirstOrDefaultAsync(f => f.TripId == trip.Id && f.ClassType == classType, cancellationToken);

            if (fare == null)
            {
                db.Fares.Add(new Fare
                {
                    TripId = trip.Id,
                    ClassType = classType,
                    Price = price,
                    Currency = "PLN"
                });
                return;
            }

            fare.Price = price;
            fare.Currency = "PLN";
        }

        private static async Task EnsureUserAsync(
            TrainTicketDbContext db,
            IConfiguration configuration,
            string passwordConfigKey,
            string email,
            string role,
            CancellationToken cancellationToken)
        {
            if (await db.Users.AnyAsync(u => u.Email == email, cancellationToken))
                return;

            var password = configuration[passwordConfigKey] ?? DefaultPassword;
            db.Users.Add(new User
            {
                Email = email,
                NormalizedEmail = email.ToUpperInvariant(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Phone = role == "Admin" ? "000-ADMIN" : "000-PASSENGER",
                Role = role
            });
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
