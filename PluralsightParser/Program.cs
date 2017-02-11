using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json.Linq;
using PluralsightParser.Components;
using PluralsightParser.Configuration;
using PluralsightParser.Dto;
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

            _config =  json.Read<PluralsightConfiguration>("config.json");

            Login(_config.Login, _config.Password);

            var videos = ParseCource("angular-2-first-look");

            foreach (var video in videos)
            {
                Download(@"D:\\test", video);
            }
        }

        private static void Login(string username, string password)
        {
            Executor.ExecutePost(_config.LoginUrl, new Dictionary<string, string>()
            {
                {"Username", username},
                {"Password", password},
            });
        }

        private static IEnumerable<Video> ParseCource(string courseId)
        {
            var payload = Executor.ExecutePost(_config.PayloadUrl, new { courseId = courseId });

            var modules = JObject.Parse(payload)["modules"].ToObject<List<Module>>();

            foreach (var video in modules.SelectMany(s => s.Clips))
            {
                var args = video.Id.Split(':');

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

                yield return new Video(video.ModuleTitle.CleanInvalidCharacters(), ++video.ModuleIndex, video.Title.CleanInvalidCharacters() + ".mp4", url);
            }
        }

        private static void Download(string localPath, Video video)
        {
            var directoryPath = Path.Combine(localPath, string.Format("{0}. {1}", video.FolderIndex, video.FolderName));

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, video.FileName);

            if (File.Exists(filePath))
            {
                return;
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var request = (HttpWebRequest)WebRequest.Create(video.Url);

                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        stream.CopyTo(fileStream);
                    }
                }
            }
        }
    }
}
