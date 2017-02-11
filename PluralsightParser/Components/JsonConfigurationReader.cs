using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace PluralsightParser.Components
{
    class JsonConfigurationReader
    {
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
                    string filePath;

                    if (!string.IsNullOrWhiteSpace(attr.FilePath))
                    {
                        filePath = $"{attr.FilePath}.json";
                    }
                    else
                    {
                        var name = type.Name.Replace("Configuration", string.Empty);

                        filePath = $"{name}.json";
                    }

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