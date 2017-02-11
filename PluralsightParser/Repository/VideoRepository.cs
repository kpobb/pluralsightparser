using System.Collections.Generic;
using PluralsightParser.Dto;

namespace PluralsightParser.Repository
{
    class VideoRepository
    {
        private readonly List<Video> _videos = new List<Video>(); 

        public void Add(Video entity)
        {
            _videos.Add(entity);
        }
    }
}
