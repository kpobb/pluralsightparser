using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace PluralsightParser.Components
{
    class JsonConfigurationReader
    {
        public T Read<T>()
        {
            var filePath = GetFilePath<T>();

            var file = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(file);
        }

        public T Read<T>(string path)
        {
            var file = File.ReadAllText(path);

            return JsonConvert.DeserializeObject<T>(file);
        }

        public object Read(string path, Type type)
        {
            var file = File.ReadAllText(path);

            return JsonConvert.DeserializeObject(file, type);
        }

        public void ReadAll()
        {
            var assembly = Assembly.GetEntryAssembly();

            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttribute<Configuration>();

                if (attr != null)
                {
                    var filePath = GetFilePath(type);

                    if (File.Exists(filePath))
                    {
                       var file = Read(filePath, type);

                        foreach (var prop in type.GetProperties())
                        {
                            if (prop.CanWrite)
                            {
                                var a = file.GetType().GetProperty(prop.Name).GetValue(file);

                                prop.SetValue(type, a);
                            }
                        }
                    }
                }
            }
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