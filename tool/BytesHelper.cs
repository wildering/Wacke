using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WackeClient.tool
{
    public static class BytesHelper
    {
        private const long KB = 1024L;
        private const long MB = 1024L * 1024L;
        private const long GB = 1024L * 1024L * 1024L;

        // 缓存 3 个常用单位，JIT 后就是 3 个引用比较
        private static readonly string[] Units = { " B", " KB", " MB", " GB" };

        public static string FormatBytes(long value)
        {
            if (value < 0) value = 0;

            long div;
            int index;      
            int decimals;   
            if (value < KB) { div = 1; index = 0; decimals = 0; }
            else if (value < MB) { div = KB; index = 1; decimals = 1; }
            else if (value < GB) { div = MB; index = 2; decimals = 1; }
            else { div = GB; index = 3; decimals = 2; }

            long whole = value / div;
            long frac = ((value % div) * (decimals == 0 ? 0 : 10)) / div;

            var sb = new System.Text.StringBuilder(16);
            sb.Append(whole);

            if (decimals > 0 && frac > 0)
            {
                sb.Append('.');
                sb.Append(frac);
            }

            sb.Append(Units[index]);
            return sb.ToString();
        }
        public static string ToKB(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            text = text.Trim().ToUpperInvariant();

            double factor = 0;
            if (text.EndsWith("GB"))
                factor = 1024 * 1024;
            else if (text.EndsWith("MB"))
                factor = 1024;
            else return null;

            string numPart = text.Substring(0, text.Length - 2).Trim();
            if (!double.TryParse(numPart, System.Globalization.NumberStyles.Float,
                                 System.Globalization.CultureInfo.InvariantCulture,
                                 out double value))
                return null;

            return ((long)(value * factor)).ToString();
        }
    }
}
