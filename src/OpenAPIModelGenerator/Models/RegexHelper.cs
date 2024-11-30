using System.Text.RegularExpressions;

namespace OpenAPIModelGenerator.Models
{
    public static partial class RegexLibrary
    {
        /// <summary>
        /// Checking for lower case characters.
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"[a-z]")]
        public static partial Regex LowerCase();

        /// <summary>
        /// Checking for numeric and white space characters.
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"[0-9\s]")]
        public static partial Regex NumbersAndWhiteSpace();

        /// <summary>
        /// Checking for characters that are not alpha numeric.
        /// </summary>
        /// <returns></returns>
        [GeneratedRegex(@"[^a-zA-Z0-9]")]
        public static partial Regex NotAlphaNumeric();
    }
}
