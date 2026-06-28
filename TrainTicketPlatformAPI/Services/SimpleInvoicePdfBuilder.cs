using System.Globalization;
using System.Text;
using TrainTicketPlatformAPI.Models;

namespace TrainTicketPlatformAPI.Services
{
    internal static class SimpleInvoicePdfBuilder
    {
        public static byte[] Build(Invoice invoice)
        {
            var stream = BuildPageStream(invoice);
            var objects = new List<string>
            {
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [5 0 R] /Count 1 >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>",
                "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold >>",
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents 6 0 R >>",
                $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream"
            };

            return BuildPdf(objects);
        }

        private static string BuildPageStream(Invoice invoice)
        {
            var targetReference = invoice.BookingOrder?.OrderReference
                ?? invoice.Booking?.BookingReference
                ?? "RailBook ticket purchase";
            var route = invoice.BookingOrder?.Bookings.FirstOrDefault()?.Trip?.TrainRoute is not null
                ? GetRoute(invoice.BookingOrder.Bookings.First())
                : invoice.Booking is not null ? GetRoute(invoice.Booking) : "RailBook journey";

            var builder = new StringBuilder();
            builder.AppendLine("0.95 0.97 0.99 rg 0 0 612 792 re f");
            builder.AppendLine("1 1 1 rg 48 70 516 650 re f");
            builder.AppendLine("0.04 0.10 0.33 RG 2 w 48 70 516 650 re S");
            builder.AppendLine("0.04 0.10 0.33 rg 48 660 516 60 re f");
            WriteText(builder, "RailBook Invoice", 72, 685, 22, bold: true, white: true);
            WriteText(builder, invoice.InvoiceNumber, 388, 688, 13, bold: true, white: true);

            WriteText(builder, "Seller", 72, 620, 13, bold: true);
            WriteText(builder, "RailBook Demo Railway Platform", 72, 598, 11);
            WriteText(builder, "Warsaw, Poland", 72, 580, 11);

            WriteText(builder, "Buyer", 330, 620, 13, bold: true);
            WriteText(builder, invoice.BuyerName, 330, 598, 11);
            WriteText(builder, invoice.BuyerEmail, 330, 580, 11);
            if (!string.IsNullOrWhiteSpace(invoice.BuyerTaxId))
                WriteText(builder, $"Tax ID: {invoice.BuyerTaxId}", 330, 562, 11);
            if (!string.IsNullOrWhiteSpace(invoice.BillingAddress))
                WriteText(builder, invoice.BillingAddress, 330, 544, 11);

            WriteText(builder, $"Issue date: {invoice.IssuedAtUtc:yyyy-MM-dd HH:mm} UTC", 72, 530, 11);
            WriteText(builder, $"Reference: {targetReference}", 72, 506, 11);
            WriteText(builder, $"Route: {route}", 72, 482, 11);

            builder.AppendLine("0.90 0.93 0.98 rg 72 394 468 42 re f");
            WriteText(builder, "Description", 88, 410, 11, bold: true);
            WriteText(builder, "Net", 326, 410, 11, bold: true);
            WriteText(builder, "VAT", 394, 410, 11, bold: true);
            WriteText(builder, "Total", 462, 410, 11, bold: true);

            WriteText(builder, "Passenger rail ticket sale", 88, 368, 11);
            WriteText(builder, FormatMoney(invoice.NetAmount, invoice.Currency), 326, 368, 11);
            WriteText(builder, FormatMoney(invoice.VatAmount, invoice.Currency), 394, 368, 11);
            WriteText(builder, FormatMoney(invoice.TotalAmount, invoice.Currency), 462, 368, 11, bold: true);

            builder.AppendLine("0.04 0.10 0.33 rg 350 292 190 48 re f");
            WriteText(builder, $"Amount due: {FormatMoney(invoice.TotalAmount, invoice.Currency)}", 366, 310, 12, bold: true, white: true);
            WriteText(builder, "Payment status: paid", 72, 290, 11, bold: true);
            WriteText(builder, "Demo invoice artifact for thesis scope.", 72, 120, 10);
            return builder.ToString();
        }

        private static string GetRoute(Booking booking)
        {
            if (booking.SegmentDepartureStation != null && booking.SegmentArrivalStation != null)
                return $"{booking.SegmentDepartureStation.Name} -> {booking.SegmentArrivalStation.Name}";

            if (booking.Trip?.TrainRoute != null)
                return $"{booking.Trip.TrainRoute.DepartureStation.Name} -> {booking.Trip.TrainRoute.ArrivalStation.Name}";

            return $"{booking.Train.DepartureStation} -> {booking.Train.ArrivalStation}";
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
            writer.Write("% RailBook invoice\n");
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

        private static string FormatMoney(decimal value, string currency)
            => $"{value.ToString("0.00", CultureInfo.InvariantCulture)} {currency}";

        private static string EscapePdfText(string value)
            => value.Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");
    }
}
