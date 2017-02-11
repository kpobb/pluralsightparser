using System;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace PluralsightParser.Extensions
{
    public static class JTokenExensions
    {
        public static string GetValidString(this JToken token)
        {
            var builder = new StringBuilder();
            var invalid = Path.GetInvalidFileNameChars();

            foreach (var symbol in token.ToString())
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