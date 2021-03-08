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
        //[Route("UsersData")]
        //[Route("UsersData/GetData")]
        [HttpGet]
        public async Task<bool> GetDataAndSaveToFiles()
        {
            await CreateJsonFile("https://jsonplaceholder.typicode.com/users", "users.json");
            await CreateJsonFile("https://jsonplaceholder.typicode.com/posts", "posts.json");
            await CreateJsonFile("https://jsonplaceholder.typicode.com/comments", "comments.json");
          
            return true;
        }

        // ⦁	Get all posts with Title , Author, Thumbnail and # of comments 
        [HttpGet("GetAllPosts")]
        public async Task<bool> GetAllPosts()
        {
            string currentPath = Directory.GetCurrentDirectory();
            string jsonFolderPath = Path.Combine(currentPath, "JsonFiles\\");
            string usersFileName = jsonFolderPath + "users.json";
            string postsFileName = jsonFolderPath + "posts.json";
            string commentsFileName = jsonFolderPath + "comments.json";

            List<User> lstUsers = ReadJsonFile<User>(usersFileName);
            List<Post> lstPosts = ReadJsonFile<Post>(postsFileName);
            List<Comment> lstComments = ReadJsonFile<Comment>(commentsFileName);

            //var result = houses
            //.Where(h => people.Count(p => p.Housenumber == h.Housenumber) >= 2)
            //.ToList();
            //listB.Where(b => listA.FirstOrDefault(a => a.AID == b.AID)?.Name == name)

            //var houseWithMorePeople =
            //   from house in houses
            //   where (
            //           from person in persons
            //           where person.Housenumber == house.Housenumber
            //           select person
            //   ).Count() > 1
            //   select house;

            //lstPosts.Where(b=>lstUsers.FirstOrDefault(a => a.id == b.userId)).

            return true;
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
