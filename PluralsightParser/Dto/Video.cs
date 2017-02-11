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
        public int FolderIndex { get; private set; }

        public string FolderName { get; private set; }

        public string FileName { get; private set; }

        public string Url { get; private set; }
    }
}