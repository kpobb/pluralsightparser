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
            PluralsightAuthentication();

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

            string courseData = null;

            try
            {
                courseData = Executor.ExecutePost(_config.PayloadUrl, new { courseId = courseId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while getting payload data. Message: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(courseData))
            {
                Console.WriteLine($"{courseUrl} was not found on the server.");
                Console.ReadKey();

                return;
            }

            Console.WriteLine("\nDownloading...\n");

            try
            {
                DownloadCourse(courseData);

                Console.WriteLine("\nDownload has been completed.\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occured while parsing the module. Message: {ex.Message}");
            }

            Console.ReadKey();
        }

        private static void PluralsightAuthentication()
        {
            Executor.ExecutePost(_config.LoginUrl, new
            {
                Username = _config.Login,
                Password = _config.Password
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

                    string url = null;
                    var moduleIndex = 0;

                    try
                    {
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

                        url = JObject.Parse(viewClip)["urls"][0]["url"].ToObject<string>();

                        int.TryParse(clip["moduleIndex"].ToString(), out moduleIndex);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error occured while getting ViewClip data. Message: {ex.Message}");
                    }

                    try
                    {
                        DownloadClip(url, clip["title"].GetValidString() + ".mp4", ++moduleIndex, module["title"].GetValidString());

                        // Just to avoid too many requests issue
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error occured while downloading the clip. Message: {ex.Message}");
                    }
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
