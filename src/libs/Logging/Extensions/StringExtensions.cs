using System.Collections;
using System.Text.RegularExpressions;

namespace Logging.Extensions
{
    public static class StringExtensions
    {
        public static string Truncate(this string data, int maxSize)
        {
            if (string.IsNullOrEmpty(data)) return data;
            if (maxSize == 0) return string.Empty;
            if (maxSize < 0) return data;

            return data.Length <= maxSize ? data : data.Substring(0, maxSize);
        }

        public static bool HasValue(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static string TransformTokens(this string input, IDictionary envData, string pattern = @"{(\w+)}")
        {
            var regex = new Regex(pattern);

            var match = regex.Match(input);
            if (!match.Success) return input;
            var fieldName = match.Groups[1].Value;
            if (envData.Contains(fieldName))
            {
                input = input.Replace(fieldName, envData[fieldName].ToString());
            }

            return input;
        }
        
        public static string ToCsv(this string[] s,string delimiter = ",")
        {
            return string.Join(delimiter,s);
        }
    }
}
