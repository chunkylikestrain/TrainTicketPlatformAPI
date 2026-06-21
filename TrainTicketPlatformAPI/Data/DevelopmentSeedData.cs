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
        private sealed record DemoCarriageSeed(
            string TrainCode,
            string Coach,
            int Position,
            string ClassType,
            string LayoutType,
            string VehicleType,
            int SeatCount,
            bool HasBikeSpace = false,
            bool HasAccessibleSpace = false,
            bool HasFamilyCompartment = false,
            bool HasDiningSection = false,
            string Notes = "");
        private sealed record RollingStockOptionSeed(
            string Category,
            string Series,
            string DisplayName,
            string Manufacturer,
            string MaxSpeed,
            int? FleetCount = null,
            int? UnitCount = null,
            string Notes = "",
            string Status = "Active");
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
            new("EIP-3508", "EIP 3508", "Express InterCity Premium", 7, 98),
            new("EIP-3510", "EIP 3510", "Express InterCity Premium", 7, 98),
            new("ED161-1610", "IC 1610 Dart", "InterCity", 8, 76),
            new("EIC-1602", "EIC 1602 Kaszub", "Express InterCity", 5, 52),
            new("IC-56", "IC 56 Wawel", "InterCity", 5, 56),
            new("IC-3806", "IC 3806 Zefir", "InterCity", 4, 56),
            new("IC-6102", "IC 6102 Heweliusz", "InterCity", 5, 56),
            new("IC-7310", "IC 7310 Malczewski", "InterCity", 4, 56),
            new("IC-3810", "IC 3810/1 Kossak", "InterCity", 8, 72),
            new("IC-8120", "IC 8120 Odra", "InterCity", 4, 56),
            new("TLK-38170", "TLK 38170 Ustronie", "Twoje Linie Kolejowe", 6, 60)
        ];

        private static readonly DemoCarriageSeed[] DemoCarriages =
        [
            new("EIP-3508", "1", 1, "Class 1", "OpenFirst", "ED250-1 first class cab unit", 54, Notes: "Fixed first-class Pendolino cab unit."),
            new("EIP-3508", "2", 2, "Class 2", "EmuSecondFamilyOpen", "ED250-2 family and open second class", 98, HasFamilyCompartment: true, Notes: "Second-class unit with family compartment and open-space seating."),
            new("EIP-3508", "3", 3, "Class 2", "EmuDiningAccessible", "ED250-3 accessible dining unit", 12, HasAccessibleSpace: true, HasDiningSection: true, Notes: "Accessible WARS dining unit with wheelchair spaces."),
            new("EIP-3508", "4", 4, "Class 2", "EmuSecondOpen", "ED250-4 second class open unit", 88, Notes: "Fixed second-class open-space unit."),
            new("EIP-3508", "5", 5, "Class 2", "EmuSecondOpen", "ED250-5 second class open unit", 88, Notes: "Fixed second-class open-space unit."),
            new("EIP-3508", "6", 6, "Class 2", "EmuSecondOpen", "ED250-6 second class open unit", 88, Notes: "Fixed second-class open-space unit."),
            new("EIP-3508", "7", 7, "Class 2", "EmuSecondQuiet", "ED250-7 quiet second class cab unit", 88, Notes: "Dedicated quiet second-class end unit, not an accessible coach."),
            new("EIP-3510", "1", 1, "Class 1", "OpenFirst", "ED250-1 first class cab unit", 54, Notes: "Fixed first-class Pendolino cab unit."),
            new("EIP-3510", "2", 2, "Class 2", "EmuSecondFamilyOpen", "ED250-2 family and open second class", 98, HasFamilyCompartment: true, Notes: "Second-class unit with family compartment and open-space seating."),
            new("EIP-3510", "3", 3, "Class 2", "EmuDiningAccessible", "ED250-3 accessible dining unit", 12, HasAccessibleSpace: true, HasDiningSection: true, Notes: "Accessible WARS dining unit with wheelchair spaces."),
            new("EIP-3510", "4", 4, "Class 2", "EmuSecondOpen", "ED250-4 second class open unit", 88, Notes: "Fixed second-class open-space unit."),
            new("EIP-3510", "5", 5, "Class 2", "EmuSecondOpen", "ED250-5 second class open unit", 88, Notes: "Fixed second-class open-space unit."),
            new("EIP-3510", "6", 6, "Class 2", "EmuSecondOpen", "ED250-6 second class open unit", 88, Notes: "Fixed second-class open-space unit."),
            new("EIP-3510", "7", 7, "Class 2", "EmuSecondQuiet", "ED250-7 quiet second class cab unit", 88, Notes: "Dedicated quiet second-class end unit, not an accessible coach."),
            new("ED161-1610", "1", 1, "Class 1", "EmuDartFirstCab", "ED161-1 first-class cab unit", 54, Notes: "First-class cab unit."),
            new("ED161-1610", "2", 2, "Class 1", "EmuDartFirstAccessible", "ED161-2 first-class accessible unit", 42, HasAccessibleSpace: true, Notes: "First-class unit with wheelchair spaces."),
            new("ED161-1610", "3", 3, "Class 2", "EmuDartRestaurant", "ED161-3 WARS restaurant unit", 16, HasDiningSection: true, Notes: "Restaurant unit with a small second-class seating section."),
            new("ED161-1610", "4", 4, "Class 2", "EmuDartSecondOpen", "ED161-4 second-class open unit", 76, Notes: "Second-class open-space unit."),
            new("ED161-1610", "5", 5, "Class 2", "EmuDartSecondOpen", "ED161-5 second-class open unit", 76, Notes: "Second-class open-space unit."),
            new("ED161-1610", "6", 6, "Class 2", "EmuDartSecondOpen", "ED161-6 second-class open unit", 76, Notes: "Second-class open-space unit."),
            new("ED161-1610", "7", 7, "Class 2", "EmuDartSecondOpen", "ED161-7 second-class open unit", 76, Notes: "Second-class open-space unit."),
            new("ED161-1610", "8", 8, "Class 2", "EmuDartSecondCab", "ED161-8 second-class cab unit", 76, Notes: "Second-class cab unit."),
            new("IC-3810", "18", 1, "Class 2", "SecondCompartment", "B10nouz 112A", 66, Notes: "Second-class compartment coach with 10 compartments."),
            new("IC-3810", "17", 2, "Class 2", "OpenSecond", "B9nopu(v)z", 88, Notes: "High-capacity second-class open-space coach with 2+2 seating."),
            new("IC-3810", "16", 3, "Class 2", "OpenSecondBike", "111Arow B7nopuvz", 72, HasBikeSpace: true, Notes: "Second-class open-space coach with bicycle racks."),
            new("IC-3810", "15", 4, "Class 2", "OpenSecond", "B9nopuvz", 88, Notes: "High-capacity second-class open-space coach with 2+2 seating."),
            new("IC-3810", "14", 5, "Class 2", "OpenSecondAccessible", "111Ainw B8bnopuz", 82, HasAccessibleSpace: true, Notes: "Second-class open-space coach with wheelchair places and accessible toilet."),
            new("IC-3810", "13", 6, "Class 2", "ComboSecondWheelchairBike", "111A-30 B6bnouvz", 64, HasBikeSpace: true, HasAccessibleSpace: true, HasFamilyCompartment: true, Notes: "Combo coach with wheelchair spaces, open seats, compartment section, and bike racks."),
            new("IC-3810", "12", 7, "Dining", "Restaurant", "WRnouz 113Aa", 0, HasDiningSection: true, Notes: "Restaurant and bar car operated by WARS."),
            new("IC-3810", "11", 8, "Class 1", "FirstCompartment", "A9nouz 140A-z", 54)
        ];

        private static readonly RollingStockOptionSeed[] RollingStockOptions =
        [
            new("Electric locomotive", "EP05", "EP05", "Skoda", "160 km/h", 1, Notes: "Modernized by ZNTK Gdansk."),
            new("Electric locomotive", "EU07", "EU07", "Pafawag / HCP", "125 km/h", 175),
            new("Electric locomotive", "EP07", "EP07", "Pafawag / HCP", "125 km/h", 175, Notes: "Modernized by ZNTKiM."),
            new("Electric locomotive", "EU07A", "EU07A", "HCP", "160 km/h", 3, Notes: "Modernized by ZNTK Olesnica / Olkol."),
            new("Electric locomotive", "EP08", "EP08", "Pafawag", "140 km/h", 9),
            new("Electric locomotive", "EP09", "EP09", "Pafawag", "160 km/h", 41),
            new("Electric locomotive", "EU44", "EU44 Husarz", "Siemens", "230 km/h", 10),
            new("Electric locomotive", "EU160", "EU160 Griffin", "Newag", "160 km/h", 96),
            new("Electric locomotive", "EU200", "EU200 Griffin", "Newag", "200 km/h", 21, Notes: "21 in service out of 78 listed in source snapshot."),
            new("Diesel locomotive", "SM42-6D", "SM42 6D", "Fablok", "90 km/h", 17),
            new("Diesel locomotive", "SM42-18D", "SM42 18D", "Fablok", "90 km/h", 10, Notes: "Modernized by Newag."),
            new("Diesel locomotive", "SU42", "SU42", "Fablok", "90 km/h", 10, Notes: "Modernized by Newag."),
            new("Diesel locomotive", "SU160", "SU160 Gama", "Pesa", "160 km/h", 10, Notes: "Type 111Db."),
            new("Diesel locomotive", "SM60", "SM60 EffiShunter 300", "CZ Loko", "60 km/h", 10),
            new("Electric multiple unit", "ED74", "ED74 Bydgostia", "Pesa", "160 km/h", 14, 4),
            new("Electric multiple unit", "ED160", "ED160 FLIRT3", "Stadler Polska", "160 km/h", 32, 8),
            new("Electric multiple unit", "ED161", "ED161 Dart", "Pesa", "160 km/h", 20, 8),
            new("Electric multiple unit", "ED250", "ED250 Pendolino", "Alstom", "250 km/h", 20, 7),
            new("Electric multiple unit", "CORADIA-MAX", "Coradia Max", "Alstom", "200 km/h", 0, 6, "Planned fleet entry; source snapshot lists 0 out of 42.")
        ];

        private static readonly DemoScheduleSeed[] DemoSchedules =
        [
            new("EIP-3508", "KRK-GDY", new TimeSpan(6, 6, 0), 360, 134m, 90m, "3", "1"),
            new("IC-56", "KRK-GDY", new TimeSpan(6, 19, 0), 381, 62m, 47m, "4", "2"),
            new("IC-3806", "KRP-GDY", new TimeSpan(6, 54, 0), 393, 62m, 47m, "4", "1"),
            new("EIP-3510", "WAW-KRK", new TimeSpan(7, 12, 0), 160, 119m, 78m, "2", "3"),
            new("IC-56", "KRK-WAW", new TimeSpan(15, 10, 0), 170, 84m, 59m, "2", "2"),
            new("ED161-1610", "RZE-WAW", new TimeSpan(12, 20, 0), 250, 92m, 64m, "2", "1"),
            new("EIC-1602", "WAW-GDN", new TimeSpan(8, 25, 0), 175, 112m, 76m, "5", "1"),
            new("IC-6102", "GDN-WAW", new TimeSpan(16, 40, 0), 185, 98m, 65m, "2", "4"),
            new("IC-7310", "RZE-WAW", new TimeSpan(5, 48, 0), 250, 92m, 64m, "1", "2"),
            new("IC-7310", "WAW-RZE", new TimeSpan(17, 35, 0), 250, 92m, 64m, "6", "1"),
            new("IC-3810", "PRZ-KOL", new TimeSpan(7, 50, 0), 683, 156m, 98m, "4", "2"),
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
            await EnsureRollingStockOptionsAsync(db, cancellationToken);

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

        private static async Task EnsureSeatsAsync(
            TrainTicketDbContext db,
            Train train,
            CancellationToken cancellationToken,
            bool refreshExistingDemoSeats = false)
        {
            var existingSeats = await db.Seats
                .Where(s => s.TrainId == train.Id)
                .ToListAsync(cancellationToken);

            if (existingSeats.Count > 0)
            {
                if (!refreshExistingDemoSeats)
                    return;

                var hasBookings = await db.Bookings
                    .AnyAsync(b => b.TrainId == train.Id, cancellationToken);

                if (hasBookings)
                    return;

                db.Seats.RemoveRange(existingSeats);
                await db.SaveChangesAsync(cancellationToken);
            }

            var carriages = await db.TrainCarriages
                .Where(c => c.TrainId == train.Id && c.SeatCount > 0)
                .OrderBy(c => c.Position)
                .ToListAsync(cancellationToken);
            var seats = new List<Seat>();

            if (carriages.Count == 0)
            {
                var carriageCount = Math.Max(1, train.CarriageCount);
                var seatsPerCarriage = Math.Max(4, train.SeatsPerCarriage);

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
            }
            else
            {
                foreach (var carriage in carriages)
                {
                    for (var number = 1; number <= carriage.SeatCount; number++)
                    {
                        seats.Add(new Seat
                        {
                            TrainId = train.Id,
                            Coach = carriage.Coach,
                            Number = number.ToString(),
                            ClassType = GetSeatClassType(carriage, number),
                            IsAvailable = true
                        });
                    }
                }
            }

            db.Seats.AddRange(seats);
            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureRollingStockOptionsAsync(
            TrainTicketDbContext db,
            CancellationToken cancellationToken)
        {
            foreach (var seed in RollingStockOptions)
            {
                var option = await db.RollingStockOptions
                    .FirstOrDefaultAsync(
                        item => item.Category == seed.Category && item.Series == seed.Series,
                        cancellationToken);

                if (option == null)
                {
                    option = new RollingStockOption
                    {
                        Category = seed.Category,
                        Series = seed.Series
                    };
                    db.RollingStockOptions.Add(option);
                }

                option.DisplayName = seed.DisplayName;
                option.Manufacturer = seed.Manufacturer;
                option.MaxSpeed = seed.MaxSpeed;
                option.FleetCount = seed.FleetCount;
                option.UnitCount = seed.UnitCount;
                option.Notes = seed.Notes;
                option.Status = seed.Status;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static string GetSeatClassType(TrainCarriage carriage, int seatNumber)
        {
            if (IsMixedClassCarriage(carriage))
                return seatNumber <= GetFirstClassSeatCount(carriage) ? "Class 1" : "Class 2";

            return carriage.ClassType == "Class 1/2" ? "Class 2" : carriage.ClassType;
        }

        private static bool IsMixedClassCarriage(TrainCarriage carriage) =>
            carriage.ClassType == "Class 1/2" ||
            carriage.LayoutType.Contains("FirstSecond", StringComparison.OrdinalIgnoreCase);

        private static int GetFirstClassSeatCount(TrainCarriage carriage)
        {
            if (carriage.LayoutType.Equals("ComboFirstSecond", StringComparison.OrdinalIgnoreCase))
                return Math.Min(18, carriage.SeatCount);

            if (carriage.LayoutType.Equals("EmuFirstSecond", StringComparison.OrdinalIgnoreCase))
                return Math.Min(16, carriage.SeatCount);

            return Math.Min(18, carriage.SeatCount);
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
                await EnsureDemoCarriagesAsync(db, train, trainSeed.Code, cancellationToken);
                await EnsureSeatsAsync(db, train, cancellationToken, refreshExistingDemoSeats: true);
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

        private static async Task EnsureDemoCarriagesAsync(
            TrainTicketDbContext db,
            Train train,
            string trainCode,
            CancellationToken cancellationToken)
        {
            var carriageSeeds = DemoCarriages
                .Where(c => c.TrainCode.Equals(trainCode, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (carriageSeeds.Length == 0)
            {
                carriageSeeds = Enumerable.Range(1, Math.Max(1, train.CarriageCount))
                    .Select(position =>
                    {
                        var classType = position == 1 ? "Class 1" : "Class 2";
                        var layoutType = position == 1 ? "FirstCompartment" : position == 2 ? "ComboAccessible" : "OpenSecond";
                        return new DemoCarriageSeed(
                            trainCode,
                            position.ToString(),
                            position,
                            classType,
                            layoutType,
                            string.Empty,
                            Math.Max(4, train.SeatsPerCarriage),
                            HasAccessibleSpace: layoutType == "ComboAccessible",
                            HasFamilyCompartment: layoutType == "ComboAccessible");
                    })
                    .ToArray();
            }

            var expectedCoaches = carriageSeeds
                .Select(seed => seed.Coach)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentCarriages = await db.TrainCarriages
                .Where(c => c.TrainId == train.Id)
                .ToListAsync(cancellationToken);
            var staleCarriages = currentCarriages
                .Where(c => !expectedCoaches.Contains(c.Coach))
                .ToList();

            if (staleCarriages.Count > 0)
                db.TrainCarriages.RemoveRange(staleCarriages);

            foreach (var seed in carriageSeeds)
            {
                var carriage = await db.TrainCarriages
                    .FirstOrDefaultAsync(
                        c => c.TrainId == train.Id && c.Coach == seed.Coach,
                        cancellationToken);

                if (carriage == null)
                {
                    carriage = new TrainCarriage
                    {
                        TrainId = train.Id,
                        Coach = seed.Coach
                    };
                    db.TrainCarriages.Add(carriage);
                }

                carriage.Position = seed.Position;
                carriage.ClassType = seed.ClassType;
                carriage.LayoutType = seed.LayoutType;
                carriage.VehicleType = seed.VehicleType;
                carriage.SeatCount = seed.SeatCount;
                carriage.HasBikeSpace = seed.HasBikeSpace;
                carriage.HasAccessibleSpace = seed.HasAccessibleSpace;
                carriage.HasFamilyCompartment = seed.HasFamilyCompartment;
                carriage.HasDiningSection = seed.HasDiningSection;
                carriage.Notes = seed.Notes;
            }

            await db.SaveChangesAsync(cancellationToken);
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
