using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ServerlessDemo.Models;

namespace ServerlessDemo
{
    public static class CrackerNewsApi
    {
        static List<Cracker> items = new List<Cracker>();

        [FunctionName("CreateCracker")]
        public static async Task<IActionResult> CreateCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = "cracker")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Creating new cracker news");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<CreateCracker>(requestBody);

            var cracker = new Cracker(){ Description = input.NewsDescription };
            items.Add(cracker);

            return new OkObjectResult(cracker);
        }

        [FunctionName("GetCrackers")]
        public static async Task<IActionResult> GetCrackers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cracker")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Getting cracker news");

            return new OkObjectResult(items);
        }

        [FunctionName("GetCracker")]
        public static async Task<IActionResult> GetCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cracker/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Getting cracker with id {id}");

            var cracker = items.FirstOrDefault(c => c.Id == id);

            return cracker is null ? (IActionResult) new NotFoundResult() : new OkObjectResult(cracker);
        }

        [FunctionName("UpdateCracker")]
        public static async Task<IActionResult> UpdateCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "cracker/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Updating cracker with id {id}");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedItem = JsonConvert.DeserializeObject<UpdateCracker>(requestBody);

            var cracker = items.FirstOrDefault(c => c.Id == id);
            if (cracker is null)
            {
                return new NotFoundResult();
            }

            cracker.IsRead = updatedItem.IsRead;
            if (!string.IsNullOrWhiteSpace(updatedItem.NewsDescription))
            {
                cracker.Description = updatedItem.NewsDescription;
            }

            return new OkObjectResult(cracker);
        }

        [FunctionName("DeleteCracker")]
        public static async Task<IActionResult> DeleteCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "cracker/{id}")] HttpRequest req,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting cracker with id {id}");

            var cracker = items.FirstOrDefault(c => c.Id == id);
            if (cracker is null)
            {
                return new NotFoundResult();
            }

            items.Remove(cracker);

            return new OkResult();
        }
    }
}
