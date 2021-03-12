using Aehu.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;

namespace Aehu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersDataController : ControllerBase
    {
        #region Fields

        private IMemoryCache _cache;
        private IConfiguration _configuration;

        #endregion

        #region Constructor

        public UsersDataController(IMemoryCache cache, IConfiguration configuration)
        {
            _cache = cache;
            _configuration = configuration;
        }

        #endregion

        #region Public methods

        [HttpGet]
        public async Task<bool> GetDataAndSaveToFiles()
        {
            try
            {
                string apiResponse = await CreateJsonFile("https://jsonplaceholder.typicode.com/users", "users.json");
                List<User> users = JsonConvert.DeserializeObject<List<User>>(apiResponse);
                _cache.Set("Users", users);

                apiResponse = await CreateJsonFile("https://jsonplaceholder.typicode.com/comments", "comments.json");
                List<Comment> comments = JsonConvert.DeserializeObject<List<Comment>>(apiResponse);
                _cache.Set("Comments", comments);

                string fileName = "posts.json";
                string postsUrl = "https://jsonplaceholder.typicode.com/posts";
                List<Post> posts = null;

                using (var httpClient = new HttpClient())
                {
                    using (var response = await httpClient.GetAsync(postsUrl))
                    {
                        string currentPath = Directory.GetCurrentDirectory();
                        string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
                        apiResponse = await response.Content.ReadAsStringAsync();
                        posts = JsonConvert.DeserializeObject<List<Post>>(apiResponse);
                        string imgUrl = "https://picsum.photos/400/400";
                        foreach (Post post in posts)
                        {
                            string imageUrl = await DownloadFile(imgUrl, post.id);
                            post.imageUrl = "http://localhost/images/" + imageUrl;
                        }

                        var postsJson = System.Text.Json.JsonSerializer.Serialize<List<Post>>(posts);
                        System.IO.File.WriteAllText(jsonFolderPath + fileName, postsJson);

                        _cache.Set("Posts", posts);
                    }//using
                }//using

                return true;
            }
            catch (Exception ex)
            {
                //Write Exception to log
                return false;
            }
            
        }

        [HttpGet("GetComments")]
        public async Task<List<Comment>> GetCommentsByPostId(int postId)
        {
            List<Comment> comments = _cache.Get<List<Comment>>("Comments");
            if (comments != null)
            {
                List<Comment> commentsByPost = comments.Where(l => l.postId == postId).Select(l => l).ToList<Comment>();
                return commentsByPost;
            }

            return null;
        }

        [HttpDelete("DeletePost")]
        public bool DeletePost(int postId)
        {
            List<Post> posts = _cache.Get<List<Post>>("Posts");
            bool isRemoved = false;

            if ( posts != null && posts.Count > 0)
            {
                Post post = posts.Find(l=>l.id == postId);
                if ( post != null)
                {
                    isRemoved = posts.Remove(post);
                    _cache.Set("Posts", posts);

                    string currentPath = Directory.GetCurrentDirectory();
                    string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
                    var postsJson = System.Text.Json.JsonSerializer.Serialize<List<Post>>(posts);
                    System.IO.File.WriteAllText(jsonFolderPath + "posts.json", postsJson);
                }
            }

            return isRemoved;
        }

        [HttpGet("GetAllPosts")]
        public async Task<List<PostData>> GetAllPosts()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
            string usersFileName = jsonFolderPath + "users.json";
            string postsFileName = jsonFolderPath + "posts.json";
            string commentsFileName = jsonFolderPath + "comments.json";
            List<User> lstUsers       = await ReadJsonFile<User>(usersFileName);
            List<Post> lstPosts       = await ReadJsonFile<Post>(postsFileName);
            List<Comment> lstComments = await ReadJsonFile<Comment>(commentsFileName);

           var posts = from post in lstPosts
                    join user in lstUsers on post.userId equals user.id
                    select new PostData
                    {
                        Id     = post.id,
                        Title  = post.title,
                        Picture = post.imageUrl,
                        Author = user.name,
                        CommentsCount = (from c in lstComments where c.postId == post.id select c).Count(),
                        Comments = (from c in lstComments where c.postId == post.id select c).ToList()
                    };

            return posts.ToList<PostData>();
        }

        #endregion

        #region Private methods

        private async Task<string> DownloadFile(string url, int id)
        {
            string fileUrl = null;
            string imagesDirectory = _configuration.GetValue<string>("ImagesDirectory");
            if ( !Directory.Exists(imagesDirectory) )
            {
                Directory.CreateDirectory(imagesDirectory);
            }

            //string currentPath = Directory.GetCurrentDirectory();
            //string imagesPath = Path.Combine(imagesDirectory, "Images");
            using (var client = new HttpClient())
            {
                using (var result = await client.GetAsync(url))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        byte[] arrBytes = await result.Content.ReadAsByteArrayAsync();
                        System.IO.File.WriteAllBytes(imagesDirectory + "\\" + id.ToString() + ".jpg", arrBytes); // Requires System.IO
                        fileUrl = id.ToString() + ".jpg";
                    }
                }
            }
            return fileUrl;
        }

        private static async Task<List<T>> ReadJsonFile<T>(string usersFileName)
        {
            List<T> lst;
            using (StreamReader r = new StreamReader(usersFileName))
            {
                string json = await r.ReadToEndAsync();
                lst = JsonConvert.DeserializeObject<List<T>>(json);
            }

            return lst;
        }

        private async Task<string> CreateJsonFile(string url, string fileName)
        {
            using (var httpClient = new HttpClient())
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    string currentPath = Directory.GetCurrentDirectory();
                    string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    System.IO.File.WriteAllText(jsonFolderPath + fileName, apiResponse);
                    return apiResponse;
                }
            }
        }

        #endregion

    }
}
