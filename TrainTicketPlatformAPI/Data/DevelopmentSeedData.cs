using System.Globalization;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TrainTicketPlatformAPI.Contracts.OpenRailway;
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
        private sealed record DiscountRuleSeed(
            string Name,
            decimal Percent,
            string EligibleClass,
            string DocumentHint,
            string Status = "Active");

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

        private static readonly DiscountRuleSeed[] DiscountRules =
        [
            new(
                "Normal Ticket",
                0m,
                "All classes",
                "No discount document required"),
            new(
                "Student 51%",
                51m,
                "All classes",
                "Valid student or doctoral student document checked during travel"),
            new(
                "Child 37%",
                37m,
                "Class 2 only",
                "Age, school, or child entitlement document checked during travel"),
            new(
                "Senior 30%",
                30m,
                "All classes",
                "Photo identification confirming age over 60 checked during travel"),
            new(
                "Senior statutory 37%",
                37m,
                "Class 2 only",
                "Pensioner or retiree entitlement document checked during travel"),
            new(
                "Big Family 30%",
                30m,
                "All classes",
                "Big Family Card or equivalent family entitlement checked during travel"),
            new(
                "Family Ticket 30%",
                30m,
                "All classes",
                "Group must include a child under 16; age document checked during travel")
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
            new("PL", "PL", "Poland"),
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
        private static string BuildStationCode(string stationName)
        {
            var normalized = stationName
                .Replace("Ł", "L", StringComparison.Ordinal)
                .Replace("ł", "l", StringComparison.Ordinal)
                .Normalize(NormalizationForm.FormD);

            var builder = new StringBuilder(normalized.Length);
            var previousWasSeparator = false;

            foreach (var character in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark)
                {
                    continue;
                }

                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                    previousWasSeparator = false;
                }
                else if (!previousWasSeparator)
                {
                    builder.Append('_');
                    previousWasSeparator = true;
                }
            }

            return builder.ToString().Trim('_');
        }

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
            new("EIC-1602", "EIC 1602 Kaszub", "Express InterCity", 7, 72),
            new("IC-56", "IC 56 Wawel", "InterCity", 6, 72),
            new("IC-3806", "IC 3806 Zefir", "InterCity", 5, 72),
            new("IC-6102", "IC 6102 Heweliusz", "InterCity", 6, 72),
            new("IC-7310", "IC 7310 Malczewski", "InterCity", 5, 72),
            new("IC-3810", "IC 3810/1 Kossak", "InterCity", 8, 72),
            new("IC-8120", "IC 8120 Odra", "InterCity", 5, 72),
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
            CancellationToken cancellationToken = default,
            string? contentRootPath = null)
        {
            if (!configuration.GetValue("SeedData:UseDevelopmentSeedData", true))
                return;

            await EnsureReferenceLocationsAsync(db, cancellationToken);
            await EnsureCleanStationDisplaysAsync(db, cancellationToken);
            await CleanupLegacyPrototypeSeedDataAsync(db, cancellationToken);
            await EnsureRollingStockOptionsAsync(db, cancellationToken);

            var loadedOpenRailwaySnapshots = await EnsureOpenRailwaySeedSnapshotsAsync(
                db,
                configuration,
                contentRootPath,
                cancellationToken);
            var useHandWrittenSchedules = configuration.GetValue("SeedData:UseHandWrittenDemoSchedules", false);
            var useHandWrittenFallback = configuration.GetValue("SeedData:UseHandWrittenDemoScheduleFallback", true);

            if (useHandWrittenSchedules || (!loadedOpenRailwaySnapshots && useHandWrittenFallback))
            {
                await EnsureDemoSchedulesAsync(db, cancellationToken);
            }
            await EnsureDiscountRulesAsync(db, cancellationToken);

            await EnsureUserAsync(db, configuration, "SeedData:AdminPassword", AdminEmail, "Admin", cancellationToken);
            await EnsureUserAsync(db, configuration, "SeedData:PassengerPassword", PassengerEmail, "Passenger", cancellationToken);

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task CleanupLegacyPrototypeSeedDataAsync(
            TrainTicketDbContext db,
            CancellationToken cancellationToken)
        {
            var legacyTrainNames = new[] { "IC 101", "IC 202" };
            var legacyTrainIds = await db.Trains
                .Where(t => legacyTrainNames.Contains(t.Name) || legacyTrainNames.Contains(t.Code))
                .Select(t => t.Id)
                .ToListAsync(cancellationToken);

            if (legacyTrainIds.Count > 0)
            {
                var legacyTripIds = await db.Trips
                    .Where(t => legacyTrainIds.Contains(t.TrainId))
                    .Select(t => t.Id)
                    .ToListAsync(cancellationToken);
                var bookedTripIds = await db.Bookings
                    .Where(b => b.TripId.HasValue && legacyTripIds.Contains(b.TripId.Value))
                    .Select(b => b.TripId!.Value)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                var removableTripIds = legacyTripIds
                    .Except(bookedTripIds)
                    .ToList();

                if (removableTripIds.Count > 0)
                {
                    var fares = await db.Fares
                        .Where(f => removableTripIds.Contains(f.TripId))
                        .ToListAsync(cancellationToken);
                    var trips = await db.Trips
                        .Where(t => removableTripIds.Contains(t.Id))
                        .ToListAsync(cancellationToken);

                    db.Fares.RemoveRange(fares);
                    db.Trips.RemoveRange(trips);
                    await db.SaveChangesAsync(cancellationToken);
                }

                var bookedTrainIds = await db.Bookings
                    .Where(b => legacyTrainIds.Contains(b.TrainId))
                    .Select(b => b.TrainId)
                    .Distinct()
                    .ToListAsync(cancellationToken);
                var removableTrainIds = legacyTrainIds
                    .Except(bookedTrainIds)
                    .ToList();

                if (removableTrainIds.Count > 0)
                {
                    var seats = await db.Seats
                        .Where(s => removableTrainIds.Contains(s.TrainId))
                        .ToListAsync(cancellationToken);
                    var carriages = await db.TrainCarriages
                        .Where(c => removableTrainIds.Contains(c.TrainId))
                        .ToListAsync(cancellationToken);
                    var trains = await db.Trains
                        .Where(t => removableTrainIds.Contains(t.Id))
                        .ToListAsync(cancellationToken);

                    db.Seats.RemoveRange(seats);
                    db.TrainCarriages.RemoveRange(carriages);
                    db.Trains.RemoveRange(trains);
                    await db.SaveChangesAsync(cancellationToken);
                }
            }

            var emptyPrototypeRoutes = await db.TrainRoutes
                .Include(r => r.Trips)
                .Include(r => r.RouteStops)
                .Where(r => string.IsNullOrWhiteSpace(r.Code) && !r.Trips.Any())
                .ToListAsync(cancellationToken);

            if (emptyPrototypeRoutes.Count > 0)
            {
                var emptyRouteStops = emptyPrototypeRoutes
                    .SelectMany(r => r.RouteStops)
                    .ToList();

                db.TrainRouteStops.RemoveRange(emptyRouteStops);
                db.TrainRoutes.RemoveRange(emptyPrototypeRoutes);
                await db.SaveChangesAsync(cancellationToken);
            }
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
            var stationCode = BuildSeedStationCode(code, name, locality.Id);
            var stationName = string.IsNullOrWhiteSpace(name) ? stationCode : name.Trim();
            var stationCity = string.IsNullOrWhiteSpace(city) ? stationName : city.Trim();

            var station = await db.Stations.FirstOrDefaultAsync(s => s.Code == stationCode, cancellationToken);
            if (station != null)
            {
                station.Name = stationName;
                station.City = stationCity;
                station.CountryId = country.Id;
                station.StateRegionId = region.Id;
                station.LocalityId = locality.Id;
                return station;
            }

            var normalizedName = stationName.Trim().ToUpperInvariant();
            var normalizedCode = stationCode.Trim().ToUpperInvariant();

            // Check local tracker first (case-insensitive name match)
            station = db.Stations.Local.FirstOrDefault(s =>
                string.Equals(s.Name?.Trim(), stationName, StringComparison.OrdinalIgnoreCase));

            if (station != null)
            {
                return station;
            }

            // Check DB by normalized name
            station = await db.Stations
                .FirstOrDefaultAsync(s => s.NormalizedName == normalizedName, cancellationToken);

            if (station != null)
            {
                return station;
            }

            station = new Station
            {
                Code = stationCode,
                NormalizedCode = normalizedCode,
                Name = stationName,
                NormalizedName = normalizedName,
                City = stationCity,
                CountryId = country.Id,
                StateRegionId = region.Id,
                LocalityId = locality.Id
            };

            db.Stations.Add(station);

            try
            {
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                // Possible unique constraint race or existing DB row; try re-querying by normalized name
                var existing = await db.Stations
                    .FirstOrDefaultAsync(s => s.NormalizedName == normalizedName, cancellationToken);
                if (existing != null)
                {
                    return existing;
                }

                throw new InvalidOperationException(
                    $"Could not seed station '{stationName}' with code '{stationCode}' and city '{stationCity}'.",
                    ex);
            }

            return station;
        }

        private static string BuildSeedStationCode(string code, string name, int localityId)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                return code.Trim();
            }

            var source = string.IsNullOrWhiteSpace(name) ? $"STATION_{localityId}" : name.Trim();
            var builder = new System.Text.StringBuilder(source.Length);

            foreach (var character in source)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            if (builder.Length == 0)
            {
                builder.Append("STATION_").Append(localityId);
            }

            return builder.Length <= 32 ? builder.ToString() : builder.ToString(0, 32);
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
                    foreach (var number in GetSeatNumbersForCarriage(carriage))
                    {
                        seats.Add(new Seat
                        {
                            TrainId = train.Id,
                            Coach = carriage.Coach,
                            Number = number,
                            ClassType = GetSeatClassType(carriage, int.Parse(number)),
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

        private static async Task EnsureDiscountRulesAsync(
            TrainTicketDbContext db,
            CancellationToken cancellationToken)
        {
            foreach (var seed in DiscountRules)
            {
                var discount = await db.DiscountRules
                    .FirstOrDefaultAsync(item => item.Name == seed.Name, cancellationToken);

                if (discount == null)
                {
                    discount = new DiscountRule { Name = seed.Name };
                    db.DiscountRules.Add(discount);
                }

                discount.Percent = seed.Percent;
                discount.EligibleClass = seed.EligibleClass;
                discount.DocumentHint = seed.DocumentHint;
                discount.Status = seed.Status;
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static string GetSeatClassType(TrainCarriage carriage, int seatNumber)
        {
            if (carriage.LayoutType.Equals("InternationalSleeper", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase))
                return "Sleeper";

            if (carriage.LayoutType.Equals("Couchette", StringComparison.OrdinalIgnoreCase) ||
                carriage.LayoutType.Equals("SixBerthCouchette", StringComparison.OrdinalIgnoreCase))
                return "Couchette";

            if (IsMixedClassCarriage(carriage))
                return seatNumber <= GetFirstClassSeatCount(carriage) ? "Class 1" : "Class 2";

            return carriage.ClassType == "Class 1/2" ? "Class 2" : carriage.ClassType;
        }

        private static IReadOnlyList<string> GetSeatNumbersForCarriage(TrainCarriage carriage)
        {
            if (carriage.LayoutType.Equals("InternationalSleeper", StringComparison.OrdinalIgnoreCase))
                return InternationalSleeperBerths;

            if (carriage.LayoutType.Equals("Sleeper", StringComparison.OrdinalIgnoreCase))
                return DomesticSleeperBerths;

            if (carriage.LayoutType.Equals("Couchette", StringComparison.OrdinalIgnoreCase))
                return FourBerthCouchetteBerths;

            if (carriage.LayoutType.Equals("SixBerthCouchette", StringComparison.OrdinalIgnoreCase))
                return SixBerthCouchetteBerths;

            return Enumerable.Range(1, carriage.SeatCount)
                .Select(number => number.ToString())
                .ToArray();
        }

        private static readonly string[] InternationalSleeperBerths =
        [
            "11", "13", "15",
            "21", "23", "25",
            "31", "33", "35",
            "41", "45",
            "51", "55",
            "61", "63", "65",
            "71", "73", "75",
            "81", "83", "85"
        ];

        private static readonly string[] DomesticSleeperBerths =
        [
            "11", "13", "15",
            "21", "23", "25",
            "31", "33", "35",
            "41", "43", "45",
            "51", "53", "55",
            "61", "63", "65",
            "71", "73", "75",
            "81", "83", "85",
            "91", "93", "95",
            "101", "103", "105"
        ];

        private static readonly string[] FourBerthCouchetteBerths =
        [
            "11", "15",
            "21", "22", "25", "26",
            "31", "32", "35", "36",
            "41", "42", "45", "46",
            "51", "52", "55", "56",
            "61", "62", "65", "66",
            "71", "72", "75", "76",
            "81", "82", "85", "86"
        ];

        private static readonly string[] SixBerthCouchetteBerths =
        [
            "11", "15",
            "21", "22", "23", "24", "25", "26",
            "31", "32", "33", "34", "35", "36",
            "41", "42", "43", "44", "45", "46",
            "51", "52", "53", "54", "55", "56",
            "61", "62", "63", "64", "65", "66",
            "71", "72", "73", "74", "75", "76",
            "81", "82", "83", "84", "85", "86"
        ];

        private static async Task<bool> EnsureOpenRailwaySeedSnapshotsAsync(
            TrainTicketDbContext db,
            IConfiguration configuration,
            string? contentRootPath,
            CancellationToken cancellationToken)
        {
            var configuredDirectory = configuration["SeedData:OpenRailwaySnapshotDirectory"];
            var snapshotDirectory = string.IsNullOrWhiteSpace(configuredDirectory)
                ? Path.Combine("App_Data", "SeedSnapshots")
                : configuredDirectory;

            if (!Path.IsPathRooted(snapshotDirectory))
            {
                snapshotDirectory = Path.Combine(
                    contentRootPath ?? Directory.GetCurrentDirectory(),
                    snapshotDirectory);
            }

            if (!Directory.Exists(snapshotDirectory))
                return false;

            var files = Directory
                .EnumerateFiles(snapshotDirectory, "*.json")
                .OrderBy(file => file, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (files.Length == 0)
                return false;

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var loadedAny = false;
            foreach (var file in files)
            {
                await using var stream = File.OpenRead(file);
                var snapshot = await JsonSerializer.DeserializeAsync<OpenRailwaySeedSnapshotDto>(
                    stream,
                    options,
                    cancellationToken);

                if (snapshot == null)
                    continue;

                await EnsureOpenRailwaySeedSnapshotAsync(db, snapshot, cancellationToken);
                loadedAny = true;
            }

            return loadedAny;
        }

        private static async Task EnsureOpenRailwaySeedSnapshotAsync(
            TrainTicketDbContext db,
            OpenRailwaySeedSnapshotDto snapshot,
            CancellationToken cancellationToken)
        {
            var externalSource = string.IsNullOrWhiteSpace(snapshot.ExternalSource)
                ? "PLK"
                : snapshot.ExternalSource.Trim();

            var stationsByExternalId = new Dictionary<int, Station>();
            var stationsByCode = new Dictionary<string, Station>(StringComparer.OrdinalIgnoreCase);

            foreach (var stationSeed in snapshot.Stations)
            {
                if (string.IsNullOrWhiteSpace(stationSeed.Code) ||
                    string.IsNullOrWhiteSpace(stationSeed.Name))
                {
                    continue;
                }

                var station = await FindSnapshotStationAsync(
                    db,
                    externalSource,
                    stationSeed.ExternalStationId,
                    stationSeed.Code,
                    cancellationToken);

                if (station == null)
                {
                    station = new Station();
                    db.Stations.Add(station);
                }

                station.Code = stationSeed.Code.Trim();
                station.NormalizedCode = station.Code.ToUpperInvariant();
                station.Name = stationSeed.Name.Trim();
                station.NormalizedName = station.Name.ToUpperInvariant();
                station.City = string.IsNullOrWhiteSpace(stationSeed.City)
                    ? station.Name
                    : stationSeed.City.Trim();
                station.ExternalSource = externalSource;
                station.ExternalStationId = stationSeed.ExternalStationId;

                if (stationSeed.ExternalStationId.HasValue)
                    stationsByExternalId[stationSeed.ExternalStationId.Value] = station;
                stationsByCode[station.Code] = station;
            }

            await db.SaveChangesAsync(cancellationToken);

            foreach (var station in await db.Stations
                .Where(station => station.ExternalSource == externalSource)
                .ToListAsync(cancellationToken))
            {
                if (station.ExternalStationId.HasValue)
                    stationsByExternalId[station.ExternalStationId.Value] = station;
                stationsByCode[station.Code] = station;
            }

            var trainsByCode = new Dictionary<string, Train>(StringComparer.OrdinalIgnoreCase);
            foreach (var trainSeed in snapshot.Trains)
            {
                if (string.IsNullOrWhiteSpace(trainSeed.Code))
                    continue;

                var train = await db.Trains
                    .FirstOrDefaultAsync(
                        item => item.Code == trainSeed.Code ||
                            (item.ExternalSource == externalSource &&
                             item.ExternalCommercialCategorySymbol == trainSeed.ExternalCommercialCategorySymbol &&
                             item.ExternalNationalNumber == trainSeed.ExternalNationalNumber &&
                             trainSeed.ExternalNationalNumber != string.Empty),
                        cancellationToken);

                if (train == null)
                {
                    train = new Train();
                    db.Trains.Add(train);
                }

                train.Code = trainSeed.Code.Trim();
                train.Name = string.IsNullOrWhiteSpace(trainSeed.Name)
                    ? train.Code
                    : trainSeed.Name.Trim();
                train.Type = string.IsNullOrWhiteSpace(trainSeed.Type)
                    ? ResolveTrainType(trainSeed.ExternalCommercialCategorySymbol)
                    : trainSeed.Type.Trim();
                train.CarriageCount = Math.Max(1, trainSeed.CarriageCount);
                train.SeatsPerCarriage = Math.Max(0, trainSeed.SeatsPerCarriage);
                train.Status = string.IsNullOrWhiteSpace(trainSeed.Status) ? "Active" : trainSeed.Status.Trim();
                train.DepartureStation = trainSeed.DepartureStation;
                train.ArrivalStation = trainSeed.ArrivalStation;
                train.DepartureTime = trainSeed.DepartureTime;
                train.ArrivalTime = trainSeed.ArrivalTime;
                train.ExternalSource = externalSource;
                train.ExternalCarrierCode = trainSeed.ExternalCarrierCode;
                train.ExternalCommercialCategorySymbol = trainSeed.ExternalCommercialCategorySymbol;
                train.ExternalNationalNumber = trainSeed.ExternalNationalNumber;
                train.ExternalInternationalArrivalNumber = trainSeed.ExternalInternationalArrivalNumber;
                train.ExternalInternationalDepartureNumber = trainSeed.ExternalInternationalDepartureNumber;

                await db.SaveChangesAsync(cancellationToken);
                await EnsureSnapshotCarriagesAsync(db, train, trainSeed, cancellationToken);
                await EnsureSeatsAsync(db, train, cancellationToken, refreshExistingDemoSeats: true);

                trainsByCode[train.Code] = train;
            }

            var routesByCode = new Dictionary<string, TrainRoute>(StringComparer.OrdinalIgnoreCase);
            foreach (var routeSeed in snapshot.Routes)
            {
                var firstStop = routeSeed.Stops.OrderBy(stop => stop.StopOrder).FirstOrDefault();
                var lastStop = routeSeed.Stops.OrderBy(stop => stop.StopOrder).LastOrDefault();
                var origin = ResolveSnapshotStation(stationsByExternalId, stationsByCode, routeSeed.DepartureExternalStationId, firstStop?.StationCode);
                var destination = ResolveSnapshotStation(stationsByExternalId, stationsByCode, routeSeed.ArrivalExternalStationId, lastStop?.StationCode);

                if (origin == null || destination == null || string.IsNullOrWhiteSpace(routeSeed.Code))
                    continue;

                var route = await db.TrainRoutes
                    .Include(item => item.RouteStops)
                    .FirstOrDefaultAsync(
                        item =>
                            item.ExternalSource == externalSource &&
                            item.ExternalScheduleId == routeSeed.ExternalScheduleId &&
                            item.ExternalOrderId == routeSeed.ExternalOrderId &&
                            item.ExternalOperatingDate == routeSeed.ExternalOperatingDate,
                        cancellationToken);

                if (route == null)
                {
                    route = await db.TrainRoutes
                        .Include(item => item.RouteStops)
                        .FirstOrDefaultAsync(item => item.Code == routeSeed.Code, cancellationToken);
                }

                if (route == null)
                {
                    route = new TrainRoute();
                    db.TrainRoutes.Add(route);
                }

                route.Code = routeSeed.Code.Trim();
                route.Name = string.IsNullOrWhiteSpace(routeSeed.Name)
                    ? $"{origin.Name} to {destination.Name}"
                    : routeSeed.Name.Trim();
                route.DepartureStationId = origin.Id;
                route.ArrivalStationId = destination.Id;
                route.DistanceKm = routeSeed.DistanceKm;
                route.EstimatedDurationMinutes = routeSeed.EstimatedDurationMinutes;
                route.OperatingDays = string.IsNullOrWhiteSpace(routeSeed.OperatingDays)
                    ? "Imported"
                    : routeSeed.OperatingDays.Trim();
                route.IntermediateStops = routeSeed.IntermediateStops;
                route.IsActive = routeSeed.IsActive;
                route.ExternalSource = externalSource;
                route.ExternalScheduleId = routeSeed.ExternalScheduleId;
                route.ExternalOrderId = routeSeed.ExternalOrderId;
                route.ExternalTrainOrderId = routeSeed.ExternalTrainOrderId;
                route.ExternalOperatingDate = routeSeed.ExternalOperatingDate;

                if (route.RouteStops.Count > 0)
                    db.TrainRouteStops.RemoveRange(route.RouteStops);

                foreach (var stopSeed in routeSeed.Stops.OrderBy(stop => stop.StopOrder))
                {
                    var station = ResolveSnapshotStation(
                        stationsByExternalId,
                        stationsByCode,
                        stopSeed.ExternalStationId,
                        stopSeed.StationCode);

                    if (station == null)
                        continue;

                    route.RouteStops.Add(new TrainRouteStop
                    {
                        StationId = station.Id,
                        StopOrder = stopSeed.StopOrder,
                        ArrivalOffsetMinutes = stopSeed.ArrivalOffsetMinutes,
                        DepartureOffsetMinutes = stopSeed.DepartureOffsetMinutes,
                        Platform = stopSeed.Platform,
                        Track = stopSeed.Track,
                        StopType = stopSeed.StopType,
                        ExternalStationId = stopSeed.ExternalStationId,
                        ExternalStopTypeId = stopSeed.ExternalStopTypeId,
                        ExternalStopTypeName = stopSeed.ExternalStopTypeName,
                        ExternalArrivalTrainNumber = stopSeed.ExternalArrivalTrainNumber,
                        ExternalDepartureTrainNumber = stopSeed.ExternalDepartureTrainNumber,
                        ArrivalDayOffset = stopSeed.ArrivalDayOffset,
                        DepartureDayOffset = stopSeed.DepartureDayOffset
                    });
                }

                await db.SaveChangesAsync(cancellationToken);
                routesByCode[route.Code] = route;
            }

            foreach (var tripSeed in snapshot.Trips)
            {
                if (!trainsByCode.TryGetValue(tripSeed.TrainCode, out var train) ||
                    !routesByCode.TryGetValue(tripSeed.RouteCode, out var route))
                {
                    continue;
                }

                var trip = await db.Trips
                    .FirstOrDefaultAsync(
                        item =>
                            item.ExternalSource == externalSource &&
                            item.ExternalScheduleId == tripSeed.ExternalScheduleId &&
                            item.ExternalOrderId == tripSeed.ExternalOrderId &&
                            item.ExternalOperatingDate == tripSeed.ExternalOperatingDate,
                        cancellationToken);

                if (trip == null)
                {
                    trip = await db.Trips
                        .FirstOrDefaultAsync(
                            item => item.TrainId == train.Id &&
                                item.TrainRouteId == route.Id &&
                                item.DepartureTime == tripSeed.DepartureTime,
                            cancellationToken);
                }

                if (trip == null)
                {
                    trip = new Trip
                    {
                        TrainId = train.Id,
                        TrainRouteId = route.Id
                    };
                    db.Trips.Add(trip);
                }

                trip.TrainId = train.Id;
                trip.TrainRouteId = route.Id;
                trip.DepartureTime = tripSeed.DepartureTime;
                trip.ArrivalTime = tripSeed.ArrivalTime;
                trip.Platform = tripSeed.Platform;
                trip.Track = tripSeed.Track;
                trip.Status = string.IsNullOrWhiteSpace(tripSeed.Status) ? "Scheduled" : tripSeed.Status;
                trip.DelayMinutes = tripSeed.DelayMinutes;
                trip.CancellationReason = tripSeed.CancellationReason;
                trip.OriginalPlatform = tripSeed.OriginalPlatform;
                trip.OriginalTrack = tripSeed.OriginalTrack;
                trip.DisruptionMessage = tripSeed.DisruptionMessage;
                trip.DisruptionSeverity = tripSeed.DisruptionSeverity;
                trip.ExternalSource = externalSource;
                trip.ExternalScheduleId = tripSeed.ExternalScheduleId;
                trip.ExternalOrderId = tripSeed.ExternalOrderId;
                trip.ExternalTrainOrderId = tripSeed.ExternalTrainOrderId;
                trip.ExternalOperatingDate = tripSeed.ExternalOperatingDate;
                trip.ExternalImportedAtUtc = snapshot.ExportedAtUtc == default ? DateTime.UtcNow : snapshot.ExportedAtUtc;
                trip.ExternalRawVersion = tripSeed.ExternalRawVersion;

                await db.SaveChangesAsync(cancellationToken);
                await EnsureSnapshotFaresAsync(db, trip, train, route, tripSeed.Fares, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task<Station?> FindSnapshotStationAsync(
            TrainTicketDbContext db,
            string externalSource,
            int? externalStationId,
            string code,
            CancellationToken cancellationToken)
        {
            if (externalStationId.HasValue)
            {
                var externalStation = await db.Stations
                    .FirstOrDefaultAsync(
                        station => station.ExternalSource == externalSource &&
                            station.ExternalStationId == externalStationId.Value,
                        cancellationToken);

                if (externalStation != null)
                    return externalStation;
            }

            return await db.Stations
                .FirstOrDefaultAsync(station => station.Code == code, cancellationToken);
        }

        private static Station? ResolveSnapshotStation(
            IReadOnlyDictionary<int, Station> stationsByExternalId,
            IReadOnlyDictionary<string, Station> stationsByCode,
            int? externalStationId,
            string? code)
        {
            if (externalStationId.HasValue &&
                stationsByExternalId.TryGetValue(externalStationId.Value, out var stationById))
            {
                return stationById;
            }

            if (!string.IsNullOrWhiteSpace(code) &&
                stationsByCode.TryGetValue(code, out var stationByCode))
            {
                return stationByCode;
            }

            return null;
        }

        private static async Task EnsureSnapshotCarriagesAsync(
            TrainTicketDbContext db,
            Train train,
            OpenRailwaySeedTrainDto trainSeed,
            CancellationToken cancellationToken)
        {
            var carriageSeeds = trainSeed.Carriages.Count > 0
                ? trainSeed.Carriages
                : BuildDefaultCarriages(train, train.Code)
                    .Select(seed => new OpenRailwaySeedCarriageDto
                    {
                        Coach = seed.Coach,
                        Position = seed.Position,
                        ClassType = seed.ClassType,
                        LayoutType = seed.LayoutType,
                        VehicleType = seed.VehicleType,
                        SeatCount = seed.SeatCount,
                        HasBikeSpace = seed.HasBikeSpace,
                        HasAccessibleSpace = seed.HasAccessibleSpace,
                        HasFamilyCompartment = seed.HasFamilyCompartment,
                        HasDiningSection = seed.HasDiningSection,
                        Notes = seed.Notes
                    })
                    .ToList();

            var hasBookings = await db.Bookings
                .AnyAsync(booking => booking.TrainId == train.Id, cancellationToken);
            var expectedCoaches = carriageSeeds
                .Select(seed => seed.Coach)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var currentCarriages = await db.TrainCarriages
                .Where(carriage => carriage.TrainId == train.Id)
                .ToListAsync(cancellationToken);

            if (!hasBookings)
            {
                var staleCarriages = currentCarriages
                    .Where(carriage => !expectedCoaches.Contains(carriage.Coach))
                    .ToList();

                if (staleCarriages.Count > 0)
                    db.TrainCarriages.RemoveRange(staleCarriages);
            }

            foreach (var seed in carriageSeeds)
            {
                var carriage = currentCarriages
                    .FirstOrDefault(item => item.Coach.Equals(seed.Coach, StringComparison.OrdinalIgnoreCase));

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

            train.CarriageCount = Math.Max(1, carriageSeeds.Count);
            train.SeatsPerCarriage = carriageSeeds
                .Where(seed => seed.SeatCount > 0)
                .Select(seed => seed.SeatCount)
                .DefaultIfEmpty(train.SeatsPerCarriage)
                .Max();

            await db.SaveChangesAsync(cancellationToken);
        }

        private static async Task EnsureSnapshotFaresAsync(
            TrainTicketDbContext db,
            Trip trip,
            Train train,
            TrainRoute route,
            IReadOnlyCollection<OpenRailwaySeedFareDto> fareSeeds,
            CancellationToken cancellationToken)
        {
            if (fareSeeds.Count > 0)
            {
                foreach (var fareSeed in fareSeeds.Where(fare => !string.IsNullOrWhiteSpace(fare.ClassType)))
                {
                    await UpsertFareAsync(
                        db,
                        trip,
                        fareSeed.ClassType,
                        fareSeed.Price,
                        cancellationToken);
                }

                return;
            }

            var (class1Price, class2Price) = EstimateSnapshotFares(train, route);
            if (ShouldCreateFirstClassFare(train))
            {
                await UpsertFareAsync(db, trip, "Class 1", class1Price, cancellationToken);
            }

            await UpsertFareAsync(db, trip, "Class 2", class2Price, cancellationToken);
        }

        private static (decimal Class1Price, decimal Class2Price) EstimateSnapshotFares(
            Train train,
            TrainRoute route)
        {
            var category = FirstNonBlank(train.ExternalCommercialCategorySymbol, train.Type, train.Code)
                .ToUpperInvariant();
            var distanceFactor = route.DistanceKm <= 0
                ? 1m
                : Math.Clamp(route.DistanceKm / 700m, 0m, 1m);
            var durationFactor = route.EstimatedDurationMinutes <= 0
                ? 1m
                : Math.Clamp(route.EstimatedDurationMinutes / 600m, 0m, 1m);
            var routeFactor = Math.Max(distanceFactor, durationFactor);

            if (category.Contains("EIP", StringComparison.OrdinalIgnoreCase) ||
                train.Code.StartsWith("EIP", StringComparison.OrdinalIgnoreCase))
            {
                return (350m, 200m);
            }

            if (category.Contains("EIC", StringComparison.OrdinalIgnoreCase) ||
                train.Code.StartsWith("EIC", StringComparison.OrdinalIgnoreCase))
            {
                return (250m, 170m);
            }

            if (category.Contains("TLK", StringComparison.OrdinalIgnoreCase) ||
                train.Type.Contains("Twoje", StringComparison.OrdinalIgnoreCase) ||
                train.Code.StartsWith("TLK", StringComparison.OrdinalIgnoreCase))
            {
                var class2 = RoundFare(20m + 50m * routeFactor);
                var class1 = RoundFare(60m + 40m * routeFactor);
                return (class1, class2);
            }

            var icClass2 = RoundFare(25m + 25m * routeFactor);
            var icClass1 = RoundFare(75m + 55m * routeFactor);
            return (icClass1, icClass2);
        }

        private static bool ShouldCreateFirstClassFare(Train train)
        {
            if (train.Type.Contains("Twoje", StringComparison.OrdinalIgnoreCase) ||
                train.Code.StartsWith("TLK", StringComparison.OrdinalIgnoreCase))
            {
                return TrainHasFirstClass(train);
            }

            return true;
        }

        private static bool TrainHasFirstClass(Train train) =>
            train.Carriages.Any(carriage =>
                carriage.ClassType.Contains("Class 1", StringComparison.OrdinalIgnoreCase) ||
                carriage.ClassType.Contains("1/2", StringComparison.OrdinalIgnoreCase));

        private static decimal RoundFare(decimal price) =>
            Math.Round(price, 0, MidpointRounding.AwayFromZero);

        private static string FirstNonBlank(params string?[] values) =>
            values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value))?.Trim() ?? string.Empty;

        private static string ResolveTrainType(string category)
        {
            if (category.Equals("EIP", StringComparison.OrdinalIgnoreCase))
                return "Express InterCity Premium";

            if (category.Equals("EIC", StringComparison.OrdinalIgnoreCase))
                return "Express InterCity";

            if (category.Equals("TLK", StringComparison.OrdinalIgnoreCase))
                return "Twoje Linie Kolejowe";

            return "InterCity";
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
                carriageSeeds = BuildDefaultCarriages(train, trainCode);
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

        private static DemoCarriageSeed[] BuildDefaultCarriages(Train train, string trainCode)
        {
            if (train.Type.Equals("Express InterCity", StringComparison.OrdinalIgnoreCase))
                return BuildEicCarriages(trainCode);

            if (train.Type.Equals("Twoje Linie Kolejowe", StringComparison.OrdinalIgnoreCase))
                return BuildTlkCarriages(trainCode);

            if (train.Code.StartsWith("ED", StringComparison.OrdinalIgnoreCase) ||
                train.Name.Contains("Dart", StringComparison.OrdinalIgnoreCase) ||
                train.Name.Contains("Flirt", StringComparison.OrdinalIgnoreCase))
            {
                return BuildGenericEmuCarriages(train, trainCode);
            }

            return BuildInterCityCarriages(trainCode);
        }

        private static DemoCarriageSeed[] BuildEicCarriages(string trainCode) =>
        [
            new(trainCode, "1", 1, "Class 1", "FirstCompartment", "A9nouz first-class compartment", 54, Notes: "First-class compartment coach."),
            new(trainCode, "2", 2, "Dining", "Restaurant", "WRnouz WARS restaurant", 0, HasDiningSection: true, Notes: "Restaurant and bar car operated by WARS."),
            new(trainCode, "3", 3, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new(trainCode, "4", 4, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, HasAccessibleSpace: true, Notes: "Second-class open coach with wheelchair spaces and accessible toilet."),
            new(trainCode, "5", 5, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks."),
            new(trainCode, "6", 6, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "High-capacity second-class open coach."),
            new(trainCode, "7", 7, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Additional second-class compartment coach for longer EIC services.")
        ];

        private static DemoCarriageSeed[] BuildInterCityCarriages(string trainCode) =>
        [
            new(trainCode, "1", 1, "Class 1/2", "ComboFirstSecond", "AB9nouz first/second combo", 54, Notes: "Mixed first and second-class coach for IC routes."),
            new(trainCode, "2", 2, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new(trainCode, "3", 3, "Class 2", "SecondFamilyCompartment", "Bmnopux family compartment", 66, HasFamilyCompartment: true, Notes: "Second-class coach with family compartment."),
            new(trainCode, "4", 4, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks and vending area."),
            new(trainCode, "5", 5, "Class 2", "OpenSecondAccessible", "B8bnopuz accessible open coach", 82, HasAccessibleSpace: true, Notes: "Second-class open coach with wheelchair spaces."),
            new(trainCode, "6", 6, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Extra second-class open coach for busier IC services.")
        ];

        private static DemoCarriageSeed[] BuildTlkCarriages(string trainCode) =>
        [
            new(trainCode, "1", 1, "Sleeper", "Sleeper", "WLAB sleeper coach", 30, Notes: "Sleeper coach used on overnight TLK services."),
            new(trainCode, "2", 2, "Couchette", "Couchette", "Bc couchette coach", 30, HasAccessibleSpace: true, Notes: "4-berth couchette coach with one accessible compartment and seven 4-berth compartments."),
            new(trainCode, "3", 3, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Second-class compartment coach."),
            new(trainCode, "4", 4, "Class 2", "OpenSecondBike", "B7nopuvz bicycle open coach", 72, HasBikeSpace: true, Notes: "Second-class open coach with bicycle racks."),
            new(trainCode, "5", 5, "Class 2", "OpenSecond", "B9nopuvz second-class open coach", 88, Notes: "Second-class open coach."),
            new(trainCode, "6", 6, "Class 2", "SecondCompartment", "B10nouz second-class compartment", 66, Notes: "Additional second-class compartment coach.")
        ];

        private static DemoCarriageSeed[] BuildGenericEmuCarriages(Train train, string trainCode)
        {
            var carriageCount = Math.Max(2, train.CarriageCount);
            var seatsPerCarriage = Math.Max(40, train.SeatsPerCarriage);
            var carriages = new List<DemoCarriageSeed>(carriageCount);

            for (var position = 1; position <= carriageCount; position++)
            {
                var isFirst = position == 1;
                var isAccessible = position == 2;
                var isFamily = position == 2;
                var isBike = position == carriageCount - 1;
                var layoutType = isFirst
                    ? "EmuFirstOpen"
                    : isAccessible
                        ? "EmuSecondAccessibleFamily"
                        : isBike
                            ? "EmuSecondBike"
                            : "EmuSecondOpen";

                carriages.Add(new DemoCarriageSeed(
                    trainCode,
                    position.ToString(),
                    position,
                    isFirst ? "Class 1" : "Class 2",
                    layoutType,
                    $"EMU unit {position}",
                    seatsPerCarriage,
                    HasBikeSpace: isBike,
                    HasAccessibleSpace: isAccessible,
                    HasFamilyCompartment: isFamily,
                    Notes: isFirst
                        ? "First-class EMU unit."
                        : "Second-class EMU unit with fixed formation seating."));
            }

            return carriages.ToArray();
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
