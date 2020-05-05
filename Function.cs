using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using ServerlessDemo.Models;

namespace ServerlessDemo
{
    public static class CrackerNewsApi
    {

        [FunctionName("CreateCracker")]
        public static async Task<IActionResult> CreateCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = "cracker")] HttpRequest req,
            [Table("crackers", Connection = "AzureWebJobsStorage")] IAsyncCollector<CrackerTableEntity> crackerTable,
            ILogger log)
        {
            log.LogInformation("Creating new cracker news");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<CreateCracker>(requestBody);

            var cracker = new Cracker(){ Description = input.NewsDescription };

            await crackerTable.AddAsync(cracker.ToTableEntity());

            return new OkObjectResult(cracker);
        }

        [FunctionName("GetCrackers")]
        public static async Task<IActionResult> GetCrackers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cracker")] HttpRequest req,
            [Table("crackers", Connection = "AzureWebJobsStorage")] CloudTable crackerTable,
            ILogger log)
        {
            log.LogInformation("Getting cracker news");

            var query = new TableQuery<CrackerTableEntity>();
            var segment = await crackerTable.ExecuteQuerySegmentedAsync(query, null);

            return new OkObjectResult(segment.Select(Mappings.ToCracker));
        }

        [FunctionName("GetCracker")]
        public static async Task<IActionResult> GetCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "cracker/{id}")] HttpRequest req,
            [Table("crackers", "CRACKER", "{id}", Connection = "AzureWebJobsStorage")] CrackerTableEntity cracker,
            ILogger log, string id)
        {
            log.LogInformation($"Getting cracker with id {id}");

            if (cracker == null)
            {
                log.LogInformation($"Item {id} not found");
            }

            return new OkObjectResult(cracker.ToCracker());
        }

        [FunctionName("UpdateCracker")]
        public static async Task<IActionResult> UpdateCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "cracker/{id}")] HttpRequest req,
            [Table("crackers", "{id}", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log, string id)
        {
            log.LogInformation($"Updating cracker with id {id}");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updatedItem = JsonConvert.DeserializeObject<UpdateCracker>(requestBody);

            var findOperation = TableOperation.Retrieve<CrackerTableEntity>("CRACKER", id);
            var findResult = await table.ExecuteAsync(findOperation);
            if (findResult.Result is null)
            {
                return new NotFoundResult();
            }

            var existingRow = (CrackerTableEntity)findResult.Result;
            existingRow.IsRead = updatedItem.IsRead;
            existingRow.Description = updatedItem.NewsDescription;

            var replaceOperation = TableOperation.Replace(existingRow);
            await table.ExecuteAsync(replaceOperation);

            return new OkObjectResult(existingRow.ToCracker());
        }

        [FunctionName("DeleteCracker")]
        public static async Task<IActionResult> DeleteCracker(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "cracker/{id}")] HttpRequest req,
            [Table("crackers", Connection = "AzureWebJobsStorage")] CloudTable table,
            ILogger log, string id)
        {
            log.LogInformation($"Deleting cracker with id {id}");

            var deleteOperation = TableOperation.Delete(new TableEntity()
            {
                PartitionKey = "CRACKER",
                RowKey = id,
                ETag = "*"
            });

            try
            {
                var deleteResult = table.ExecuteAsync(deleteOperation);
            }
            catch (StorageException e) when (e.RequestInformation.HttpStatusCode == 404)
            {
                return new NotFoundResult();
            }

            return new OkResult();
        }
    }
}
