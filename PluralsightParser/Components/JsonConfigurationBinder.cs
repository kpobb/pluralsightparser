using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace PluralsightParser.Components
{
    class JsonConfigurationBinder
    {
        public T Bind<T>()
        {
            var filePath = GetFilePath<T>();

            var file = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(file);
        }

        private string GetFilePath(Type type)
        {
            var attr = type.GetCustomAttribute<Configuration>();

            if (!string.IsNullOrWhiteSpace(attr.FilePath))
            {
                return $"{attr.FilePath}.json";
            }

            var name = type.Name.Replace("Configuration", string.Empty);

            return $"{name}.json";
        }

        private string GetFilePath<T>()
        {
            return GetFilePath(typeof (T));
        }
    }

    public class Configuration : Attribute
    {
        public string FilePath { get; set; }

        public Configuration(string filePath)
        {
            FilePath = filePath;
        }

        public Configuration()
        {
        }
    }
}