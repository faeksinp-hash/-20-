using System;
using System.Globalization;

namespace Porjai20.Common
{
    /// <summary>
    /// Centralized date and time conversion helper for Porjai 20 system.
    /// Handles Thai Buddhist Era (พ.ศ.) and Christian Era (ค.ศ.) conversions,
    /// invariant string formatting, and dual-calendar SQLite query parameters.
    /// </summary>
    public static class DateTimeHelper
    {
        public const string SqliteDateTimeFormat = "yyyy-MM-dd HH:mm:ss";
        public const string SqliteDateFormat = "yyyy-MM-dd";
        public const string DisplayDateFormat = "dd/MM/yyyy";
        public const string DisplayDateTimeFormat = "dd/MM/yyyy HH:mm:ss";

        private static readonly string[] RecognizedDateFormats = {
            "yyyy-MM-dd HH:mm:ss.FFFFFFF",
            "yyyy-MM-dd HH:mm:ss",
            "yyyy-MM-ddTHH:mm:ss",
            "yyyy-MM-dd",
            "dd/MM/yyyy HH:mm:ss",
            "dd/MM/yyyy",
            "yyyy/MM/dd HH:mm:ss",
            "yyyy/MM/dd",
            "d/M/yyyy HH:mm:ss",
            "d/M/yyyy",
            "yyyy-M-d HH:mm:ss",
            "yyyy-M-d"
        };

        /// <summary>
        /// Normalizes any date to Christian Era (CE) year (e.g. 2567 -> 2024).
        /// </summary>
        public static DateTime ToChristianEra(DateTime dt)
        {
            if (dt.Year > 2400)
            {
                return dt.AddYears(-543);
            }
            return dt;
        }

        /// <summary>
        /// Converts Christian Era (CE) date to Buddhist Era (BE) year (e.g. 2024 -> 2567).
        /// </summary>
        public static DateTime ToBuddhistEra(DateTime dt)
        {
            if (dt.Year <= 2400)
            {
                return dt.AddYears(543);
            }
            return dt;
        }

        /// <summary>
        /// Formats a DateTime to Buddhist Era (BE) with the specified format string (e.g. "dd/MM/yyyy HH:mm").
        /// </summary>
        public static string ToBuddhistEra(DateTime dt, string format)
        {
            var be = ToBuddhistEra(dt);
            return be.ToString(format, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime to SQLite standard ISO string (yyyy-MM-dd HH:mm:ss) in InvariantCulture.
        /// </summary>
        public static string ToSqliteIso(DateTime dt)
        {
            var ce = ToChristianEra(dt);
            return ce.ToString(SqliteDateTimeFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Formats a DateTime to SQLite standard date string (yyyy-MM-dd) in InvariantCulture.
        /// </summary>
        public static string ToSqliteDate(DateTime dt)
        {
            var ce = ToChristianEra(dt);
            return ce.ToString(SqliteDateFormat, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Parses an arbitrary date string into normalized Christian Era (CE) DateTime.
        /// Tries exact formats, invariant culture, and Thai culture.
        /// </summary>
        public static DateTime? ParseDateToInvariant(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;
            dateStr = dateStr.Trim();

            if (DateTime.TryParseExact(dateStr, RecognizedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtExact))
            {
                return ToChristianEra(dtExact);
            }

            if (DateTime.TryParse(dateStr, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dtInv))
            {
                return ToChristianEra(dtInv);
            }

            var thaiCulture = new CultureInfo("th-TH");
            if (DateTime.TryParse(dateStr, thaiCulture, DateTimeStyles.None, out var dtThai))
            {
                return ToChristianEra(dtThai);
            }

            return null;
        }

        /// <summary>
        /// Generates dual-calendar SQLite query boundary parameters (CE and BE).
        /// Returns (StartCE, EndCE, StartBE, EndBE).
        /// </summary>
        public static (string StartCE, string EndCE, string StartBE, string EndBE) GetDualDateBoundaries(DateTime start, DateTime end)
        {
            DateTime startCE = ToChristianEra(start.Date);
            DateTime endCE = end == DateTime.MaxValue
                ? DateTime.MaxValue
                : ToChristianEra(end.Date.AddDays(1).AddTicks(-1));

            string sCe = startCE.ToString(SqliteDateFormat, CultureInfo.InvariantCulture);
            string eCe = endCE == DateTime.MaxValue
                ? "9999-12-31 23:59:59"
                : endCE.ToString(SqliteDateTimeFormat, CultureInfo.InvariantCulture);

            int startYr = startCE.Year;
            int endYr = endCE == DateTime.MaxValue ? 9999 : endCE.Year;

            string sBe = (startYr + 543).ToString("D4") + startCE.ToString("-MM-dd", CultureInfo.InvariantCulture);
            string eBe = endCE == DateTime.MaxValue
                ? "9999-12-31 23:59:59"
                : (endYr + 543).ToString("D4") + endCE.ToString("-MM-dd 23:59:59", CultureInfo.InvariantCulture);

            return (sCe, eCe, sBe, eBe);
        }
    }
}
