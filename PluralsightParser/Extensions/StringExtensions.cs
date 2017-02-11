using System.IO;
using System.Linq;
using System.Text;

namespace PluralsightParser.Extensions
{
    public static class StringExtensions
    {
        public static string CleanInvalidCharacters(this string fileName)
        {
            var builder = new StringBuilder();
            var invalid = Path.GetInvalidFileNameChars();

            foreach (var symbol in fileName)
            {
                if (!invalid.Contains(symbol))
                {
                    builder.Append(symbol);
                }
            }
            return builder.ToString();
        }
    }
}