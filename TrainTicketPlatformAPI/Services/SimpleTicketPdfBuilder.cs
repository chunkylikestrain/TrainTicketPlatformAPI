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

            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>"
            };

            var pageObjectNumbers = new List<int>();
            foreach (var (ticket, qrMatrix) in tickets)
            {
                var pageStream = BuildPageStream(ticket, qrMatrix);
                var pageObjectNumber = objects.Count + 1;
                var contentObjectNumber = objects.Count + 2;
                pageObjectNumbers.Add(pageObjectNumber);
                objects.Add($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentObjectNumber} 0 R >>");
                objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(pageStream)} >>\nstream\n{pageStream}\nendstream");
            }

            objects[1] = $"<< /Type /Pages /Kids [{string.Join(" ", pageObjectNumbers.Select(number => $"{number} 0 R"))}] /Count {tickets.Count} >>";

            return BuildPdf(objects);
        }

        private static string BuildPageStream(TicketArtifactDto ticket, IReadOnlyList<BitArray> qrMatrix)
        {
            var builder = new StringBuilder();
            builder.AppendLine("0.95 0.97 0.99 rg 0 0 612 792 re f");
            builder.AppendLine("1 1 1 rg 48 86 516 620 re f");
            builder.AppendLine("0.04 0.19 0.29 RG 2 w 48 86 516 620 re S");
            builder.AppendLine("0.04 0.19 0.29 rg 48 646 516 60 re f");
            WriteText(builder, "RailWay Ticket", 72, 670, 22, bold: true, white: true);
            WriteText(builder, $"Ticket {ticket.TicketNumber}", 380, 674, 13, bold: true, white: true);
            WriteText(builder, ticket.Route, 72, 610, 18, bold: true);
            WriteText(builder, $"Train: {ticket.TrainName}", 72, 575, 12);
            WriteText(builder, $"Passenger: {ticket.PassengerName}", 72, 552, 12);
            WriteText(builder, $"Seat: {ticket.SeatLabel}", 72, 529, 12);
            WriteText(builder, $"Travel date: {ticket.TravelDate:yyyy-MM-dd}", 72, 506, 12);
            WriteText(builder, $"Departure: {FormatDateTime(ticket.DepartureTime)}", 72, 483, 12);
            WriteText(builder, $"Arrival: {FormatDateTime(ticket.ArrivalTime)}", 72, 460, 12);
            WriteText(builder, $"Booking reference: {ticket.BookingReference}", 72, 437, 12);
            WriteText(builder, $"Issued: {ticket.IssuedAtUtc:yyyy-MM-dd HH:mm} UTC", 72, 414, 12);
            WriteText(builder, "Scan the QR code during inspection.", 72, 360, 11);
            DrawQr(builder, qrMatrix, 372, 416, 144);
            WriteText(builder, "Demo ticket artifact for thesis scope.", 72, 128, 10);
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
            builder.AppendLine($"({EscapePdfText(text)}) Tj");
            builder.AppendLine("ET");
        }

        private static byte[] BuildPdf(IReadOnlyList<string> objects)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.Write("%PDF-1.4\n");
            writer.Write("% RailWay ticket\n");
            writer.Flush();

            var offsets = new List<long> { 0 };
            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(stream.Position);
                writer.Write($"{index + 1} 0 obj\n");
                writer.Write(objects[index]);
                writer.Write("\nendobj\n");
                writer.Flush();
            }

            var xrefOffset = stream.Position;
            writer.Write($"xref\n0 {objects.Count + 1}\n");
            writer.Write("0000000000 65535 f \n");
            foreach (var offset in offsets.Skip(1))
                writer.Write($"{offset:0000000000} 00000 n \n");

            writer.Write("trailer\n");
            writer.Write($"<< /Size {objects.Count + 1} /Root 1 0 R >>\n");
            writer.Write("startxref\n");
            writer.Write($"{xrefOffset}\n");
            writer.Write("%%EOF");
            writer.Flush();

            return stream.ToArray();
        }

        private static string FormatDateTime(DateTime? value)
            => value.HasValue ? value.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture) : "n/a";

        private static string Format(decimal value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static string EscapePdfText(string value)
            => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
