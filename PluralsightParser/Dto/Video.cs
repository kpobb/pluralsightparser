namespace PluralsightParser.Dto
{
    public class Video
    {
        public Video(string folderName, int folderIndex, string fileName, string url)
        {
            FolderName = folderName;
            FolderIndex = folderIndex;
            FileName = fileName;
            Url = url;
        }

        public Video()
        {
        }

        public int FolderIndex { get; set; }

        public string FolderName { get; set; }

        public string FileName { get; set; }

        public string Url { get; set; }
    }
}