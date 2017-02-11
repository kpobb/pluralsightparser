using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;
using PluralsightParser.Components;
using PluralsightParser.Configuration;
using PluralsightParser.Extensions;

namespace PluralsightParser
{
    class Program
    {
        private static readonly HttpRequestExecutor Executor = new HttpRequestExecutor();
        private static PluralsightConfiguration _config;
        static void Main()
        {
            var json = new JsonConfigurationReader();

            _config = json.Read<PluralsightConfiguration>();

            Login(_config.Login, _config.Password);

            DownloadCourse("csharp-6-from-scratch");
        }

        private static void Login(string username, string password)
        {
            Executor.ExecutePost(_config.LoginUrl, new Dictionary<string, string>()
            {
                {"Username", username},
                {"Password", password},
            });
        }

        private static void DownloadCourse(string courseId)
        {
            var payload = Executor.ExecutePost(_config.PayloadUrl, new { courseId = courseId });

            var modules = JObject.Parse(payload)["modules"].Children();

            foreach (var module in modules)
            {
                foreach (var clip in module["clips"])
                {
                    var args = clip["id"].ToString().Split(':');

                    var viewClip = Executor.ExecutePost(_config.ViewClipUrl,
                        new
                        {
                            author = args[3],
                            clipIndex = int.Parse(args[2]),
                            courseName = args[0],
                            includeCaptions = false,
                            locale = "en",
                            mediaType = "mp4",
                            moduleName = args[1],
                            quality = "1280x720",
                        });

                    var url = JObject.Parse(viewClip)["urls"][0]["url"].ToObject<string>();

                    int moduleIndex;
                    int.TryParse(clip["moduleIndex"].ToString(), out moduleIndex);

                    DownloadClip(url, ++moduleIndex, module["title"].ToString(), clip["title"].ToString().CleanInvalidCharacters() + ".mp4");

                    // Just to avoid too many requests issue
                    Thread.Sleep(1000);
                }
            }
        }

        private static void DownloadClip(string url, int index, string folderName, string fileName)
        {
            var directoryPath = Path.Combine(_config.DownloadLocation, $"{index}. {folderName}");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, fileName);

            if (File.Exists(filePath))
            {
                return;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var request = (HttpWebRequest)WebRequest.Create(url);

                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        stream?.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}
