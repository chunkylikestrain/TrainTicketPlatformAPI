using System.Text;

namespace TrainTicketPlatformAPI.Services
{
    internal static class SimplePdfWriter
    {
        public static List<byte[]> BuildUnicodeFontObjects()
        {
            var regularFontPath = ResolveFontPath("arial.ttf", "segoeui.ttf", "calibri.ttf");
            var boldFontPath = ResolveFontPath("arialbd.ttf", "segoeuib.ttf", "calibrib.ttf") ?? regularFontPath;

            return new List<byte[]>
            {
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
        }

        public static byte[] BuildPdf(IReadOnlyList<byte[]> objects, string comment)
        {
            using var stream = new MemoryStream();
            using var writer = new StreamWriter(stream, Encoding.ASCII, leaveOpen: true);

            writer.Write("%PDF-1.4\n");
            writer.Write($"% {comment}\n");
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

        public static string EncodeText(string value)
            => Convert.ToHexString(Encoding.BigEndianUnicode.GetBytes(value));

        public static byte[] PdfObject(string value)
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
    }
}
