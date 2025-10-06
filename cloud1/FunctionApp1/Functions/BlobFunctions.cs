using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions
{
    public class BlobFunctions
    {
        private readonly ILogger<BlobFunctions> _logger;

        public BlobFunctions(ILogger<BlobFunctions> logger)
        {
            _logger = logger;
        }

        [Function("ProcessProductImage")]
        public void ProcessProductImage(
            [QueueTrigger("product-image-queue")] string queueMessage,
            FunctionContext context)
        {
            try
            {
                var message = JsonSerializer.Deserialize<BlobMessage>(queueMessage);
                _logger.LogInformation("Processing product image: {BlobName}", message.BlobName);

                // Add your image processing logic here
                // You can use BlobClient to access the blob if needed

                _logger.LogInformation("Successfully processed product image: {BlobName}", message.BlobName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing product image from queue message: {Message}", queueMessage);
                throw;
            }
        }

        private class BlobMessage
        {
            public string BlobName { get; set; } = string.Empty;
            public string ContainerName { get; set; } = "product-images";
        }
    }
}