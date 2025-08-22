using System;
using System.IO;
using System.Text;

namespace LogScrub.Gui
{
    public static class FileHelpers
    {
        public static bool IsLikelyTextByExtension(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext is ".log" or ".txt" or ".csv" or ".json" or ".xml"
                     or ".conf" or ".ini" or ".cfg" or ".properties"
                     or ".yaml" or ".yml";
        }

        public static bool LooksBinaryHead(string path, int headBytes)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                var len = (int)Math.Min(headBytes, fs.Length);
                var buf = new byte[len];
                var read = fs.Read(buf, 0, len);
                for (int i = 0; i < read; i++) if (buf[i] == 0) return true; // NUL -> binary
                return false;
            }
            catch { return true; } // treat unreadable as "not text"
        }

        public static (Encoding? enc, bool bom) DetectEncoding(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (fs.Length >= 3)
            {
                Span<byte> b = stackalloc byte[3];
                fs.Read(b);
                fs.Position = 0;
                if (b[0] == 0xEF && b[1] == 0xBB && b[2] == 0xBF) return (new UTF8Encoding(true), true);
            }
            if (fs.Length >= 2)
            {
                Span<byte> b2 = stackalloc byte[2];
                fs.Read(b2);
                fs.Position = 0;
                if (b2[0] == 0xFF && b2[1] == 0xFE) return (Encoding.Unicode, true);          // UTF-16 LE
                if (b2[0] == 0xFE && b2[1] == 0xFF) return (Encoding.BigEndianUnicode, true); // UTF-16 BE
            }
            return (null, false);
        }
    }
}
