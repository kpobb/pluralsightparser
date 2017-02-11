using System.Collections.Generic;

namespace PluralsightParser.Dto
{
    public class Module
    {
        public List<Clip> Clips { get; set; }
        public string Author { get; set; }
    }

    public class Clip
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public int ModuleIndex { get; set; }
        public string ModuleTitle { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
    }
}