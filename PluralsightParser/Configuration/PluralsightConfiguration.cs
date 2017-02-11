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
    }
}