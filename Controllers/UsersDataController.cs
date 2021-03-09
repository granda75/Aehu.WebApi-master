using Aehu.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;

namespace Aehu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersDataController : ControllerBase
    {
        
        [HttpGet]
        public async Task<bool> GetDataAndSaveToFiles()
        {
            //await CreateJsonFile("https://jsonplaceholder.typicode.com/users", "users.json");

            string fileName = "posts.json";
            string postsUrl = "https://jsonplaceholder.typicode.com/posts";
            List<Post> posts = null;

            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(postsUrl))
                {
                    string currentPath = Directory.GetCurrentDirectory();
                    string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    posts = JsonConvert.DeserializeObject<List<Post>>(apiResponse);
                    string imgUrl = "https://picsum.photos/400/400";
                    foreach (Post post in posts)
                    {
                        string imageUrl =  await DownloadFile(imgUrl, post.id);
                        post.imageUrl = imageUrl;
                    }
                    //JsonConvert.Se
                    //System.IO.File.WriteAllText(jsonFolderPath + fileName, apiResponse);
                }
            }

            //await CreateJsonFile("https://jsonplaceholder.typicode.com/posts", "posts.json");
            //await CreateJsonFile("https://jsonplaceholder.typicode.com/comments", "comments.json");

            return true;
        }

        private async Task<string> DownloadFile(string url, int id)
        {
            string fileUrl = null;
            string currentPath = Directory.GetCurrentDirectory();
            string imagesPath = Path.Combine(currentPath, "Images");
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        byte[] arrBytes = await result.Content.ReadAsByteArrayAsync();
                        System.IO.File.WriteAllBytes(imagesPath + "\\" + id.ToString() + ".jpg", arrBytes); // Requires System.IO
                        fileUrl = id.ToString() + ".jpg";
                    }
                }
            }
            return fileUrl;
        }

        // ⦁	Get all posts with Title , Author, Thumbnail and # of comments 
        [HttpGet("GetAllPosts")]
        public async Task<List<PostData>> GetAllPosts()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
            string usersFileName = jsonFolderPath + "users.json";
            string postsFileName = jsonFolderPath + "posts.json";
            string commentsFileName = jsonFolderPath + "comments.json";
            List<User> lstUsers       = ReadJsonFile<User>(usersFileName);
            List<Post> lstPosts       = ReadJsonFile<Post>(postsFileName);
            List<Comment> lstComments = ReadJsonFile<Comment>(commentsFileName);
            //1. title from post
            //2. author from User (username)
            //3. Thumbnail
            //4. number of comments

            //groups = (from g in db.Groups
            //          join gu in db.GroupUsers on g.GroupId equals gu.GroupId
            //          where g.Active == true && gu.UserId == userId
            //          select new Group
            //          {
            //              GroupId = g.GroupId,
            //              Name = g.Name,
            //              CreatedOn = g.CreatedOn,
            //              ContactCount = (from c in db.Contacts where c.OwnerGroupId == g.GroupId select c).Count(),
            //              MemberCount = (from guu in db.GroupUsers
            //                             where guu.GroupId == g.GroupId
            //                             join u in db.Users on guu.UserId equals u.UserId
            //                             where u.Active == true
            //                             select gu).Count()
            //          }).ToList<Group>();

          //  join comment in lstComments on post.id equals comment.postId

           var posts = from post in lstPosts
                    join user in lstUsers on post.userId equals user.id
                    select new PostData
                    {
                        Id     = post.id,
                        Title  = post.title,
                        Author = user.name,
                        CommentsCount = (from c in lstComments where c.postId == post.id select c).Count()
                    };

            return posts.ToList<PostData>();
        }

        private static List<T> ReadJsonFile<T>(string usersFileName)
        {
            List<T> lst;
            using (StreamReader r = new StreamReader(usersFileName))
            {
                string json = r.ReadToEnd();
                lst = JsonConvert.DeserializeObject<List<T>>(json);
            }

            return lst;
        }

        private async Task CreateJsonFile(string url, string fileName)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    string currentPath = Directory.GetCurrentDirectory();
                    string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    System.IO.File.WriteAllText(jsonFolderPath + fileName, apiResponse);
                }
            }
        }

        
        

    }
}
