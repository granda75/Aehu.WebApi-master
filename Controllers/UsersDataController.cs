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
using Aehu.Entities;
using Aehu.BL;

namespace Aehu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersDataController : ControllerBase
    {
        #region Fields

        private IMemoryCache _cache;
        private IConfiguration _configuration;
        private IPostsBL _bl;

        #endregion

        #region Constructor

        public UsersDataController(IMemoryCache cache, IConfiguration configuration, IPostsBL postsBl)
        {
            _cache = cache;
            _configuration = configuration;
            _bl = postsBl;
        }

        #endregion

        #region Public methods

        [HttpGet("RefreshData")]
        public async Task<bool> RefreshData()
        {
            string imagesDirectory = _configuration.GetValue<string>("ImagesDirectory");
            return await _bl.RefreshData(imagesDirectory);
            
        }

        [HttpGet]
        public async Task<bool> GetDataAndSaveToFiles()
        {
            string imagesDirectory = _configuration.GetValue<string>("ImagesDirectory");
            return await _bl.RefreshData(imagesDirectory);
                 
        }

        [HttpGet("GetComments")]
        public List<Comment> GetCommentsByPostId(int postId)
        {
            List<Comment> comments = _cache.Get<List<Comment>>("Comments");
            if (comments != null)
            {
                List<Comment> commentsByPost = comments.Where(l => l.postId == postId).Select(l => l).ToList<Comment>();
                return commentsByPost;
            }

            return null;
        }

        [HttpPost("AddComment")]
        public bool AddComment(Comment comment)
        {
            List<Comment> comments = _cache.Get("Comments") as List<Comment>;
            if (comments != null)
            {
                int maxId = comments.Select(l => l.id).Max();
                comment.id = maxId + 1;
                comments.Add(comment);
                _cache.Set("Comments", comments);
                return true;
            }
            return false;
        }

        [HttpDelete("DeleteComment")]
        public bool DeleteComment(int commentId)
        {
            List<Comment> comments = _cache.Get("Comments") as List<Comment>;
            if (comments != null)
            {
                Comment foundedComment = comments.Find(l => l.id == commentId);
                comments.Remove(foundedComment);
                _cache.Set("Comments", comments);

                string currentPath = Directory.GetCurrentDirectory();
                string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
                var commentsJson = System.Text.Json.JsonSerializer.Serialize<List<Comment>>(comments);
                System.IO.File.WriteAllText(jsonFolderPath + "comments.json", commentsJson);

                return true;
            }
            return false;
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
            return await _bl.GetAllPosts();
        }

        #endregion

        #region Private methods


        #endregion

    }
}
