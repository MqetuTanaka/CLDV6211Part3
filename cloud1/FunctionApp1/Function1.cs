using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FunctionApp1.Functions
{
    public class QueueProcessorFunction
    {
        private readonly ILogger<QueueProcessorFunction> _logger;

        public QueueProcessorFunction(ILogger<QueueProcessorFunction> logger)
        {
            _logger = logger;
        }

        // Listener for order-notifications queue
        [Function("OrderNotificationFunction")]
        public void ProcessOrderNotification(
            [QueueTrigger("order-notifications", Connection = "cloud")] string message,
            FunctionContext context)
        {
            _logger.LogInformation("Received message from 'order-notifications': {Message}", message);

            try
            {
                var order = JsonSerializer.Deserialize<OrderNotificationModel>(message);
                if (order != null)
                {
                    _logger.LogInformation("Order ID: {OrderId}, Status: {Status}, Customer: {CustomerName}, Total: {TotalPrice:C}",
                        order.OrderId, order.Status, order.CustomerName, order.TotalPrice);

                    // Add your order processing logic here
                    ProcessOrderBusinessLogic(order);
                }
                else
                {
                    _logger.LogWarning("OrderNotificationModel is null after deserialization.");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization failed for order notification message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order notification message.");
                throw; // Re-throw to trigger retry
            }
        }

        // Listener for stock-updates queue
        [Function("StockUpdateFunction")]
        public void ProcessStockUpdate(
            [QueueTrigger("stock-updates", Connection = "cloud")] string message,
            FunctionContext context)
        {
            _logger.LogInformation("Received message from 'stock-updates': {Message}", message);

            try
            {
                var stockUpdate = JsonSerializer.Deserialize<StockUpdateModel>(message);
                if (stockUpdate != null)
                {
                    _logger.LogInformation("Stock Update - Product: {ProductName}, Old: {PreviousStock}, New: {NewStock}, By: {UpdatedBy}",
                        stockUpdate.ProductName, stockUpdate.PreviousStock, stockUpdate.NewStock, stockUpdate.UpdatedBy);

                    // Add your stock processing logic here
                    ProcessStockBusinessLogic(stockUpdate);
                }
                else
                {
                    _logger.LogWarning("StockUpdateModel is null after deserialization.");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization failed for stock update message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process stock update message.");
                throw; // Re-throw to trigger retry
            }
        }

        // Listener for product image processing
        [Function("ProductImageFunction")]
        public void ProcessProductImage(
            [QueueTrigger("product-image-queue", Connection = "cloud")] string message,
            FunctionContext context)
        {
            _logger.LogInformation("Received message from 'product-image-queue': {Message}", message);

            try
            {
                var imageMessage = JsonSerializer.Deserialize<ProductImageModel>(message);
                if (imageMessage != null)
                {
                    _logger.LogInformation("Product Image - Blob: {BlobName}, Container: {ContainerName}",
                        imageMessage.BlobName, imageMessage.ContainerName);

                    // Add your image processing logic here
                    ProcessImageBusinessLogic(imageMessage);
                }
                else
                {
                    _logger.LogWarning("ProductImageModel is null after deserialization.");
                }
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON deserialization failed for product image message.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process product image message.");
                throw; // Re-throw to trigger retry
            }
        }

        // Business logic methods
        private void ProcessOrderBusinessLogic(OrderNotificationModel order)
        {
            _logger.LogInformation("Processing business logic for order {OrderId}", order.OrderId);

            // Implement your order processing logic here
            switch (order.Status?.ToLower())
            {
                case "completed":
                    _logger.LogInformation("Order {OrderId} completed successfully. Sending confirmation.", order.OrderId);
                    break;
                case "cancelled":
                    _logger.LogWarning("Order {OrderId} was cancelled. Processing refund if needed.", order.OrderId);
                    break;
                case "shipped":
                    _logger.LogInformation("Order {OrderId} has been shipped. Tracking notification sent.", order.OrderId);
                    break;
                default:
                    _logger.LogInformation("Order {OrderId} status updated to: {Status}", order.OrderId, order.Status);
                    break;
            }
        }

        private void ProcessStockBusinessLogic(StockUpdateModel stockUpdate)
        {
            _logger.LogInformation("Processing stock update for product {ProductName}", stockUpdate.ProductName);

            // Check if stock is low and trigger alerts
            if (stockUpdate.NewStock < 10)
            {
                _logger.LogWarning("LOW STOCK ALERT: {ProductName} has only {NewStock} units remaining",
                    stockUpdate.ProductName, stockUpdate.NewStock);
            }

            if (stockUpdate.NewStock == 0)
            {
                _logger.LogError("OUT OF STOCK: {ProductName} is now out of stock", stockUpdate.ProductName);
            }

            // Check for significant stock changes
            var stockDifference = stockUpdate.PreviousStock - stockUpdate.NewStock;
            if (Math.Abs(stockDifference) > 50)
            {
                _logger.LogInformation("Significant stock change for {ProductName}: {Difference} units",
                    stockUpdate.ProductName, stockDifference);
            }
        }

        private void ProcessImageBusinessLogic(ProductImageModel imageMessage)
        {
            _logger.LogInformation("Processing image {BlobName} from container {ContainerName}",
                imageMessage.BlobName, imageMessage.ContainerName);

            // Implement your image processing logic here
            // Example: Validate image, generate thumbnails, update database records, etc.

            // Simulate image validation
            var validExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = System.IO.Path.GetExtension(imageMessage.BlobName).ToLower();

            if (Array.Exists(validExtensions, ext => ext == fileExtension))
            {
                _logger.LogInformation("Image {BlobName} validated successfully. Extension: {Extension}",
                    imageMessage.BlobName, fileExtension);
            }
            else
            {
                _logger.LogWarning("Invalid image format for {BlobName}. Extension: {Extension}",
                    imageMessage.BlobName, fileExtension);
            }

            _logger.LogInformation("Image {BlobName} processing completed", imageMessage.BlobName);
        }
    }

    // Model for order-notifications
    public class OrderNotificationModel
    {
        public string Type { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Model for stock-updates
    public class StockUpdateModel
    {
        public string Type { get; set; } = string.Empty;
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdateDate { get; set; }
    }

    // Model for product image processing
    public class ProductImageModel
    {
        public string BlobName { get; set; } = string.Empty;
        public string ContainerName { get; set; } = "product-images";
        public DateTime UploadTime { get; set; } = DateTime.UtcNow;
    }
}