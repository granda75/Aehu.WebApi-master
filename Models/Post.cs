using System.Collections.Generic;


namespace Aehu.WebApi.Models
{
    public class Post
    {
        public int userId { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string body { get; set; }

        public string imageUrl { get; set; }

        
    }

}
