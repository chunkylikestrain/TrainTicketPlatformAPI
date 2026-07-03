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

            var regularFontPath = ResolveFontPath("arial.ttf", "segoeui.ttf", "calibri.ttf");
            var boldFontPath = ResolveFontPath("arialbd.ttf", "segoeuib.ttf", "calibrib.ttf") ?? regularFontPath;
            var objects = new List<byte[]>
            {
                PdfObject("<< /Type /Catalog /Pages 2 0 R >>"),
                PdfObject(""),
                PdfObject("<< /Type /Font /Subtype /Type0 /BaseFont /RailBook-Regular /Encoding /Identity-H /DescendantFonts [5 0 R] >>"),
                PdfObject("<< /Type /Font /Subtype /Type0 /BaseFont /RailBook-Bold /Encoding /Identity-H /DescendantFonts [6 0 R] >>"),
                PdfObject("<< /Type /Font /Subtype /CIDFontType2 /BaseFont /RailBook-Regular /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor 7 0 R /CIDToGIDMap 11 0 R /DW 600 >>"),
                PdfObject("<< /Type /Font /Subtype /CIDFontType2 /BaseFont /RailBook-Bold /CIDSystemInfo << /Registry (Adobe) /Ordering (Identity) /Supplement 0 >> /FontDescriptor 8 0 R /CIDToGIDMap 12 0 R /DW 600 >>"),
                PdfObject(BuildFontDescriptor("RailBook-Regular", regularFontPath is null ? null : 9)),
                PdfObject(BuildFontDescriptor("RailBook-Bold", boldFontPath is null ? null : 10)),
                BuildFontFileObject(regularFontPath),
                BuildFontFileObject(boldFontPath),
                BuildCidToGidMapObject(regularFontPath),
                BuildCidToGidMapObject(boldFontPath)
            };

            var pageObjectNumbers = new List<int>();
            foreach (var (ticket, qrMatrix) in tickets)
            {
                var pageStream = BuildPageStream(ticket, qrMatrix);
                var pageObjectNumber = objects.Count + 1;
                var contentObjectNumber = objects.Count + 2;
                pageObjectNumbers.Add(pageObjectNumber);
                objects.Add(PdfObject($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {contentObjectNumber} 0 R >>"));
                objects.Add(PdfObject($"<< /Length {Encoding.ASCII.GetByteCount(pageStream)} >>\nstream\n{pageStream}\nendstream"));
            }

            objects[1] = PdfObject($"<< /Type /Pages /Kids [{string.Join(" ", pageObjectNumbers.Select(number => $"{number} 0 R"))}] /Count {tickets.Count} >>");

            return BuildPdf(objects);
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
            builder.AppendLine($"<{EncodePdfText(text)}> Tj");
            builder.AppendLine("ET");
        }

        private static byte[] BuildPdf(IReadOnlyList<byte[]> objects)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.Write("%PDF-1.4\n");
            writer.Write("% RailBook ticket\n");
            writer.Flush();

            var offsets = new List<long> { 0 };
            for (var index = 0; index < objects.Count; index++)
            {
                offsets.Add(stream.Position);
                writer.Write($"{index + 1} 0 obj\n");
                writer.Flush();
                stream.Write(objects[index]);
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

        private static string EncodePdfText(string value)
            => Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(value));

        private static byte[] PdfObject(string value)
            => Encoding.ASCII.GetBytes(value);

        private static string BuildFontDescriptor(string fontName, int? fontFileObjectNumber)
        {
            var fontFileReference = fontFileObjectNumber.HasValue
                ? $" /FontFile2 {fontFileObjectNumber.Value} 0 R"
                : string.Empty;

            return $"<< /Type /FontDescriptor /FontName /{fontName} /Flags 32 /FontBBox [-664 -324 2000 1039] /ItalicAngle 0 /Ascent 905 /Descent -212 /CapHeight 716 /StemV 80{fontFileReference} >>";
        }

        private static byte[] BuildFontFileObject(string? path)
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return PdfObject("<< /Length 0 /Length1 0 >>\nstream\n\nendstream");

            var fontBytes = File.ReadAllBytes(path);
            var header = Encoding.ASCII.GetBytes($"<< /Length {fontBytes.Length} /Length1 {fontBytes.Length} >>\nstream\n");
            var footer = Encoding.ASCII.GetBytes("\nendstream");
            var result = new byte[header.Length + fontBytes.Length + footer.Length];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(fontBytes, 0, result, header.Length, fontBytes.Length);
            Buffer.BlockCopy(footer, 0, result, header.Length + fontBytes.Length, footer.Length);
            return result;
        }

        private static byte[] BuildCidToGidMapObject(string? path)
        {
            var map = BuildIdentityCidToGidMap();
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                var fontBytes = File.ReadAllBytes(path);
                map = BuildCidToGidMap(fontBytes);
            }

            var header = Encoding.ASCII.GetBytes($"<< /Length {map.Length} >>\nstream\n");
            var footer = Encoding.ASCII.GetBytes("\nendstream");
            var result = new byte[header.Length + map.Length + footer.Length];
            Buffer.BlockCopy(header, 0, result, 0, header.Length);
            Buffer.BlockCopy(map, 0, result, header.Length, map.Length);
            Buffer.BlockCopy(footer, 0, result, header.Length + map.Length, footer.Length);
            return result;
        }

        private static byte[] BuildIdentityCidToGidMap()
        {
            var map = new byte[65536 * 2];
            for (var code = 0; code <= ushort.MaxValue; code++)
                WriteUInt16BigEndian(map, code * 2, (ushort)code);

            return map;
        }

        private static byte[] BuildCidToGidMap(byte[] fontBytes)
        {
            var cmapOffset = FindTableOffset(fontBytes, "cmap");
            if (cmapOffset < 0)
                return BuildIdentityCidToGidMap();

            var subtableOffset = FindFormat4CmapOffset(fontBytes, cmapOffset);
            if (subtableOffset < 0)
                return BuildIdentityCidToGidMap();

            var map = new byte[65536 * 2];
            var segCount = ReadUInt16BigEndian(fontBytes, subtableOffset + 6) / 2;
            var endCodeOffset = subtableOffset + 14;
            var startCodeOffset = endCodeOffset + segCount * 2 + 2;
            var idDeltaOffset = startCodeOffset + segCount * 2;
            var idRangeOffsetOffset = idDeltaOffset + segCount * 2;

            for (var segmentIndex = 0; segmentIndex < segCount; segmentIndex++)
            {
                var endCode = ReadUInt16BigEndian(fontBytes, endCodeOffset + segmentIndex * 2);
                var startCode = ReadUInt16BigEndian(fontBytes, startCodeOffset + segmentIndex * 2);
                var idDelta = ReadInt16BigEndian(fontBytes, idDeltaOffset + segmentIndex * 2);
                var idRangeOffsetPosition = idRangeOffsetOffset + segmentIndex * 2;
                var idRangeOffset = ReadUInt16BigEndian(fontBytes, idRangeOffsetPosition);

                for (var code = startCode; code <= endCode && code <= ushort.MaxValue; code++)
                {
                    var glyphId = idRangeOffset == 0
                        ? (ushort)((code + idDelta) & 0xFFFF)
                        : ReadMappedGlyphId(fontBytes, idRangeOffsetPosition, idRangeOffset, code, startCode, idDelta);

                    WriteUInt16BigEndian(map, code * 2, glyphId);

                    if (code == ushort.MaxValue)
                        break;
                }
            }

            return map;
        }

        private static ushort ReadMappedGlyphId(
            byte[] fontBytes,
            int idRangeOffsetPosition,
            ushort idRangeOffset,
            int code,
            ushort startCode,
            short idDelta)
        {
            var glyphIndexOffset = idRangeOffsetPosition + idRangeOffset + (code - startCode) * 2;
            if (glyphIndexOffset < 0 || glyphIndexOffset + 1 >= fontBytes.Length)
                return 0;

            var glyphId = ReadUInt16BigEndian(fontBytes, glyphIndexOffset);
            return glyphId == 0
                ? (ushort)0
                : (ushort)((glyphId + idDelta) & 0xFFFF);
        }

        private static int FindTableOffset(byte[] fontBytes, string tableTag)
        {
            if (fontBytes.Length < 12)
                return -1;

            var tableCount = ReadUInt16BigEndian(fontBytes, 4);
            for (var index = 0; index < tableCount; index++)
            {
                var recordOffset = 12 + index * 16;
                if (recordOffset + 15 >= fontBytes.Length)
                    return -1;

                var tag = Encoding.ASCII.GetString(fontBytes, recordOffset, 4);
                if (tag == tableTag)
                    return (int)ReadUInt32BigEndian(fontBytes, recordOffset + 8);
            }

            return -1;
        }

        private static int FindFormat4CmapOffset(byte[] fontBytes, int cmapOffset)
        {
            if (cmapOffset < 0 || cmapOffset + 3 >= fontBytes.Length)
                return -1;

            var encodingRecordCount = ReadUInt16BigEndian(fontBytes, cmapOffset + 2);
            var fallbackOffset = -1;
            for (var index = 0; index < encodingRecordCount; index++)
            {
                var recordOffset = cmapOffset + 4 + index * 8;
                if (recordOffset + 7 >= fontBytes.Length)
                    return fallbackOffset;

                var platformId = ReadUInt16BigEndian(fontBytes, recordOffset);
                var encodingId = ReadUInt16BigEndian(fontBytes, recordOffset + 2);
                var subtableOffset = cmapOffset + (int)ReadUInt32BigEndian(fontBytes, recordOffset + 4);
                if (subtableOffset + 1 >= fontBytes.Length || ReadUInt16BigEndian(fontBytes, subtableOffset) != 4)
                    continue;

                if (platformId == 3 && (encodingId == 1 || encodingId == 10))
                    return subtableOffset;

                fallbackOffset = subtableOffset;
            }

            return fallbackOffset;
        }

        private static ushort ReadUInt16BigEndian(byte[] bytes, int offset)
            => (ushort)((bytes[offset] << 8) | bytes[offset + 1]);

        private static short ReadInt16BigEndian(byte[] bytes, int offset)
            => unchecked((short)ReadUInt16BigEndian(bytes, offset));

        private static uint ReadUInt32BigEndian(byte[] bytes, int offset)
            => ((uint)bytes[offset] << 24) | ((uint)bytes[offset + 1] << 16) | ((uint)bytes[offset + 2] << 8) | bytes[offset + 3];

        private static void WriteUInt16BigEndian(byte[] bytes, int offset, ushort value)
        {
            bytes[offset] = (byte)(value >> 8);
            bytes[offset + 1] = (byte)(value & 0xFF);
        }

        private static string? ResolveFontPath(params string[] fileNames)
        {
            var fontsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);
            if (string.IsNullOrWhiteSpace(fontsDirectory))
                return null;

            return fileNames
                .Select(fileName => Path.Combine(fontsDirectory, fileName))
                .FirstOrDefault(File.Exists);
        }

        private static string EscapePdfComment(string value)
            => value.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal);
    }
}
