using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json;
using System.Collections.Generic;
using Company.Services;
using Company.Models;

namespace Company.Function
{
    public static class CreateContactsForCountries
    {
        [FunctionName("CreateContactsForCountries")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "contacts-for-countries")] HttpRequest req,
            ILogger log)
        {
            var _loggingService = new LoggingService(log);

            // get secrets to access cosmos db and set up cosmos db client
            var cosmosConnectionString = Environment.GetEnvironmentVariable("databaseConnectionString");
            var cosmos = new CosmosClient(cosmosConnectionString);
            var emailsContainer = cosmos.GetContainer("EmailForwarding", "ContactInfoAndRequests");

            // Get and deserialize the request body
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var contactsForCountries = JsonSerializer.Deserialize<List<ContactsForCountry>>(requestBody);
            try
            {
                // add the new items to the database
                foreach (var contactsForCountry in contactsForCountries)
                {
                    var response = await emailsContainer.CreateItemAsync<ContactsForCountry>(contactsForCountry);
                }
            }
            catch (Exception e)
            {
                // log the exception and return a 500 error to the caller
                _loggingService.LogException(e, requestBody);
                return new StatusCodeResult(500);
            }
            return new OkObjectResult(contactsForCountries);
        }
    }
}
