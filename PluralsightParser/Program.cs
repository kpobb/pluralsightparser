using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using PluralsightParser.Dto;
using PluralsightParser.Extensions;
using System.Net;

namespace PluralsightParser
{
    class Program
    {
        private static readonly HttpRequestExecutor Executor = new HttpRequestExecutor();

        static void Main()
        {
            Login("alex-alexc8", "sxds917K");

            var videos = GetCourse("angular-2-first-look");

            foreach (var video in videos)
            {
                Download(@"D:\\test", video);
            }
        }

        private static void Login(string username, string password)
        {
            Executor.ExecutePost("https://app.pluralsight.com/id/", new Dictionary<string, string>()
            {
                {"Username", username},
                {"Password", password},
            });
        }

        private static IEnumerable<Video> GetCourse(string courseId)
        {
            var payload = Executor.ExecutePost("https://app.pluralsight.com/player/user/api/v1/player/payload", new { courseId = courseId });

            var modules = JObject.Parse(payload)["modules"].ToObject<List<Module>>();

            foreach (var video in modules.SelectMany(s => s.Clips))
            {
                var args = video.Id.Split(':');

                var viewClip = Executor.ExecutePost("https://app.pluralsight.com/video/clips/viewclip",
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
