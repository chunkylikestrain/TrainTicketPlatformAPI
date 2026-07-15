using System.Collections;
using System.Globalization;
using System.Text;
using TrainTicketPlatformAPI.Contracts.Tickets;

namespace TrainTicketPlatformAPI.Services
{
    internal static class SimpleTicketPdfBuilder
    {
        public static byte[] Build(TicketArtifactDto ticket, IReadOnlyList<BitArray> qrMatrix)
            => Build(new[] { (ticket, qrMatrix) });

        public static byte[] Build(IReadOnlyList<(TicketArtifactDto Ticket, IReadOnlyList<BitArray> QrMatrix)> tickets)
        {
            if (tickets.Count == 0)
                throw new InvalidOperationException("At least one ticket is required to build a PDF");

            var objects = new List<byte[]>
            {
                SimplePdfWriter.PdfObject("<< /Type /Catalog /Pages 2 0 R >>"),
                SimplePdfWriter.PdfObject("")
            };
            objects.AddRange(SimplePdfWriter.BuildUnicodeFontObjects());

            var pageObjectNumbers = new List<int>();
            foreach (var (ticket, qrMatrix) in tickets)
            {
                var pageStream = BuildPageStream(ticket, qrMatrix);
                var pageObjectNumber = objects.Count + 1;
                var contentObjectNumber = objects.Count + 2;
                pageObjectNumbers.Add(pageObjectNumber);
                objects.Add(SimplePdfWriter.PdfObject($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentObjectNumber} 0 R >>"));
                objects.Add(SimplePdfWriter.PdfObject($"<< /Length {Encoding.ASCII.GetByteCount(pageStream)} >>\nstream\n{pageStream}\nendstream"));
            }

            objects[1] = SimplePdfWriter.PdfObject($"<< /Type /Pages /Kids [{string.Join(" ", pageObjectNumbers.Select(number => $"{number} 0 R"))}] /Count {tickets.Count} >>");

            return SimplePdfWriter.BuildPdf(objects, "RailBook ticket");
        }

        private static string BuildPageStream(TicketArtifactDto ticket, IReadOnlyList<BitArray> qrMatrix)
        {
            var builder = new StringBuilder();
            builder.AppendLine("0.95 0.97 0.99 rg 0 0 612 792 re f");
            builder.AppendLine("1 1 1 rg 48 86 516 620 re f");
            builder.AppendLine("0.04 0.19 0.29 RG 2 w 48 86 516 620 re S");
            builder.AppendLine("0.04 0.19 0.29 rg 48 646 516 60 re f");
            WriteText(builder, "RailBook Ticket", 72, 670, 22, bold: true, white: true);
            WriteText(builder, $"Ticket {ticket.TicketNumber}", 380, 674, 13, bold: true, white: true);
            WriteText(builder, ticket.Route, 72, 610, 18, bold: true);
            WriteText(builder, $"Journey: {GetJourneyLabel(ticket)}", 72, 580, 12, bold: true);
            WriteText(builder, $"Train: {ticket.TrainName}", 72, 557, 12);
            WriteText(builder, $"Passenger: {ticket.PassengerName}", 72, 534, 12);
            WriteText(builder, $"Seat: {ticket.SeatLabel}", 72, 511, 12);
            WriteText(builder, $"Travel date: {ticket.TravelDate:yyyy-MM-dd}", 72, 488, 12);
            WriteText(builder, $"Departure: {FormatDateTime(ticket.DepartureTime)}", 72, 465, 12);
            WriteText(builder, $"Arrival: {FormatDateTime(ticket.ArrivalTime)}", 72, 442, 12);
            WriteText(builder, $"Booking reference: {ticket.BookingReference}", 72, 419, 12);
            WriteText(builder, $"Issued: {ticket.IssuedAtUtc:yyyy-MM-dd HH:mm} UTC", 72, 396, 12);
            WriteText(builder, $"Extras: {GetExtrasLabel(ticket)}", 72, 373, 12, bold: ticket.DogTicketCount > 0 || ticket.LargeBaggageTicketCount > 0);
            WriteText(builder, "Scan the QR code during inspection.", 72, 360, 11);
            DrawQr(builder, qrMatrix, 372, 416, 144);
            WriteText(builder, "Demo ticket artifact for thesis scope.", 72, 128, 10);
            builder.AppendLine($"% Ticket {EscapePdfComment(ticket.TicketNumber)}");
            builder.AppendLine($"% Journey: {EscapePdfComment(GetJourneyLabel(ticket))}");
            return builder.ToString();
        }

        private static void DrawQr(StringBuilder builder, IReadOnlyList<BitArray> matrix, decimal x, decimal y, decimal size)
        {
            if (matrix.Count == 0)
                return;

            builder.AppendLine("1 1 1 rg");
            builder.AppendLine($"{Format(x)} {Format(y)} {Format(size)} {Format(size)} re f");
            builder.AppendLine("0 0 0 rg");

            var moduleSize = size / matrix.Count;
            for (var row = 0; row < matrix.Count; row++)
            {
                var modules = matrix[row];
                for (var column = 0; column < modules.Count; column++)
                {
                    if (!modules[column])
                        continue;

                    var moduleX = x + column * moduleSize;
                    var moduleY = y + (matrix.Count - row - 1) * moduleSize;
                    builder.AppendLine($"{Format(moduleX)} {Format(moduleY)} {Format(moduleSize)} {Format(moduleSize)} re f");
                }
            }
        }

        private static void WriteText(
            StringBuilder builder,
            string text,
            int x,
            int y,
            int size,
            bool bold = false,
            bool white = false)
        {
            builder.AppendLine(white ? "1 1 1 rg" : "0.08 0.10 0.14 rg");
            builder.AppendLine("BT");
            builder.AppendLine($"/{(bold ? "F2" : "F1")} {size} Tf");
            builder.AppendLine($"{x} {y} Td");
            builder.AppendLine($"<{SimplePdfWriter.EncodeText(text)}> Tj");
            builder.AppendLine("ET");
        }

        private static string FormatDateTime(DateTime? value)
            => value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : "n/a";

        private static string GetJourneyLabel(TicketArtifactDto ticket)
        {
            var direction = string.Equals(ticket.JourneyDirection, "Return", StringComparison.OrdinalIgnoreCase)
                ? "Return"
                : "Outbound";
            return $"{direction} segment {ticket.JourneySegmentIndex + 1}";
        }

        private static string GetExtrasLabel(TicketArtifactDto ticket)
        {
            var extras = new List<string>();
            if (ticket.DogTicketCount > 0)
                extras.Add($"{ticket.DogTicketCount} dog ticket");
            if (ticket.LargeBaggageTicketCount > 0)
                extras.Add($"{ticket.LargeBaggageTicketCount} large baggage ticket");

            return extras.Count == 0
                ? "None"
                : $"{string.Join(", ", extras)} ({ticket.ExtraChargeAmount.ToString("0.00", CultureInfo.InvariantCulture)} PLN)";
        }

        private static string Format(decimal value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static string EscapePdfComment(string value)
            => value.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
    }
}
