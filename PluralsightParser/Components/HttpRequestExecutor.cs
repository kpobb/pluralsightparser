using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace PluralsightParser.Components
{
    public class HttpRequestExecutor
    {
        private enum HttpMethod
        {
            GET,
            POST
        }

        private readonly CookieContainer _cookies = new CookieContainer();

        public string ExecuteGet(string url)
        {
            return ExecuteRequest(url, HttpMethod.GET, null);
        }

        public string ExecutePost(string url, IReadOnlyDictionary<string, string> data)
        {
            return ExecuteRequest(url, HttpMethod.POST, "application/x-www-form-urlencoded", ConvertToBytes(data));
        }

        public string ExecutePost(string url, object json)
        {
            return ExecuteRequest(
                url,
                HttpMethod.POST,
                "application/json;charset=UTF-8",
                ConvertToBytes(JsonConvert.SerializeObject(json)));
        }

        private byte[] ConvertToBytes(IReadOnlyDictionary<string, string> data)
        {
            var queryString = HttpUtility.ParseQueryString(String.Empty);
            data.ToList().ForEach(s => queryString.Add(s.Key, s.Value));
            return Encoding.ASCII.GetBytes(queryString.ToString());
        }

        private byte[] ConvertToBytes(string json)
        {
            return Encoding.ASCII.GetBytes(json);
        }

        private string ExecuteRequest(string url, HttpMethod method, string contentType, byte[] data = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.CookieContainer = _cookies;
            request.Method = method.ToString();
            request.ContentType = contentType;

            switch (method)
            {
                case HttpMethod.POST:
                    {
                        if (data != null)
                        {
                            request.ContentLength = data.Length;

                            using (var postStream = request.GetRequestStream())
                            {
                                postStream.Write(data, 0, data.Length);
                                postStream.Flush();
                            }
                        }

                        break;
                    }
            }

            using (var response = request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    if (stream == null)
                    {
                        return null;
                    }

                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
