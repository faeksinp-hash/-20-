using System.Text.RegularExpressions;

namespace Porjai20.Common
{
    /// <summary>
    /// Centralized data validation helper for Porjai 20 system.
    /// Manages compiled regular expressions and standard validation rules.
    /// </summary>
    public static class ValidationHelper
    {
        // Sale receipt reference pattern: SALE-YYYYMMDDHHMMSS (14 digits timestamp)
        private static readonly Regex SaleReferenceRegex = new(@"^SALE-\d{14}$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // Thai phone number pattern: 08x-xxx-xxxx, 09x-xxx-xxxx, 06x-xxx-xxxx, 02-xxx-xxxx (9-10 digits, optional hyphens/spaces)
        private static readonly Regex PhoneNumberRegex = new(@"^0\d{1,2}-?\d{3}-?\d{4}$|^0\d{8,9}$", RegexOptions.Compiled);

        /// <summary>
        /// Validates that a string is not null, empty, or whitespace.
        /// </summary>
        public static bool IsRequired(string? input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }

        /// <summary>
        /// Validates sale order reference number pattern (SALE-XXXXXXXXXXXXXX).
        /// </summary>
        public static bool IsSaleReference(string? input)
        {
            return !string.IsNullOrWhiteSpace(input) && SaleReferenceRegex.IsMatch(input.Trim());
        }

        /// <summary>
        /// Validates standard phone number format.
        /// </summary>
        public static bool IsPhoneNumber(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            string cleaned = input.Trim().Replace(" ", "");
            return PhoneNumberRegex.IsMatch(cleaned);
        }

        /// <summary>
        /// Validates invoice / delivery note number for goods receipt.
        /// Requires non-empty input without format restriction.
        /// </summary>
        public static bool IsValidDeliveryNote(string? input)
        {
            return !string.IsNullOrWhiteSpace(input);
        }
    }
}
