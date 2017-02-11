using System.Collections.Generic;
using System.Linq;

namespace PluralsightParser.Configuration
{
    [Components.Configuration("config")]
    class PluralsightConfiguration
    {
        public string Host { get; set; }

        public string PayloadUrl => $"{Host}/player/user/api/v1/player/payload";

        public string ViewClipUrl => $"{Host}/video/clips/viewclip";

        public string LoginUrl => $"{Host}/id/";

        public string Login { get; set; }
        public string Password { get; set; }
        public string DownloadLocation { get; set; }

        public bool IsValid(out ValidationResult result)
        {
            var invliadFields = new List<string>();

            foreach (var prop in GetType().GetProperties())
            {
                var value = prop.GetValue(this);

                if (value is string && string.IsNullOrWhiteSpace(value.ToString()))
                {
                    invliadFields.Add(prop.Name);
                }
                else if (value == null)
                {
                    invliadFields.Add(prop.Name);
                }
            }

            var hasInvalidFields = invliadFields.Any();

            result = new ValidationResult(!hasInvalidFields, invliadFields);

            return !hasInvalidFields;
        }

        public bool IsValid()
        {
            ValidationResult result;

            return IsValid(out result);
        }

        public class ValidationResult
        {
            public IList<string> Fields { get; }
            public bool IsValid { get;  }

            public ValidationResult(bool isValid, IList<string> fields)
            {
                IsValid = isValid;
                Fields = fields;
            }

            public ValidationResult(bool isValid)
            {
                IsValid = isValid;
            }
        }
    }
}