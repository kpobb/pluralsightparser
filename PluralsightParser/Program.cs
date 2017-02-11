using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            Console.WriteLine("Pluralsight parser - created by -=Tj=-\n");

            _config = new JsonConfigurationBinder().Bind<PluralsightConfiguration>();

            Console.WriteLine("Checking the configuration...\n");

            if (_config == null)
            {
                Console.WriteLine("Please configurate the application.");
                Console.ReadKey();

                return;
            }

            PluralsightConfiguration.ValidationResult result;

            if (!_config.IsValid(out result))
            {
                Console.WriteLine($"The {string.Join(", ", result.Fields)} field(s) is not configured properly.");
                Console.ReadKey();

                return;
            }

            Console.WriteLine("Authenticating...\n");
            Login(_config.Login, _config.Password);

            if (!Executor.HasCookies)
            {
                Console.WriteLine("Error: Login or password is incorrect.\n");
                Console.ReadKey();

                return;
            }

            string courseUrl = null;

            while (string.IsNullOrWhiteSpace(courseUrl))
            {
                Console.Write("Enter course url: ");
                courseUrl = Console.ReadLine();
            }

            var courseId = ExtractCourseId(courseUrl);

            var courseData = Executor.ExecutePost(_config.PayloadUrl, new { courseId = courseId });

            if (string.IsNullOrWhiteSpace(courseData))
            {
                Console.WriteLine($"{courseUrl} was not found on the server.");
                Console.ReadKey();

                return;
            }

            Console.WriteLine("\nDownloading...\n");

            DownloadCourse(courseData);

            Console.WriteLine("\nCompleted.\n");
            Console.ReadKey();
        }

        private static void Login(string username, string password)
        {
            Executor.ExecutePost(_config.LoginUrl, new Dictionary<string, string>
            {
                {"Username", username},
                {"Password", password},
            });
        }

        private static void DownloadCourse(string courseData)
        {
            var modules = JObject.Parse(courseData)["modules"].Children();

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

                    DownloadClip(url, clip["title"].GetValidString() + ".mp4", ++moduleIndex, module["title"].GetValidString());

                    // Just to avoid too many requests issue
                    Thread.Sleep(1000);
                }
            }
        }

        private static void DownloadClip(string clipUrl, string clipName, int moduleIndex, string moduleFolderName)
        {
            var directoryPath = Path.Combine(_config.DownloadLocation, $"{moduleIndex}. {moduleFolderName}");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, clipName);

            if (File.Exists(filePath))
            {
                return;
            }

            Console.WriteLine(filePath);

            using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                var request = (HttpWebRequest)WebRequest.Create(clipUrl);

                using (var response = request.GetResponse())
                {
                    using (var stream = response.GetResponseStream())
                    {
                        stream?.CopyTo(fileStream);
                    }
                }
            }
        }

        private static string ExtractCourseId(string courseUrl)
        {
            return courseUrl.Split('/').Last();
        }
    }
}
