// Controllers/CustomerController.cs
using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;
using Azure;

namespace ABCRetailers.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IAzureStorageService _storageService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(IAzureStorageService storageService, ILogger<CustomerController> logger)
        {
            _storageService = storageService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var customers = await _storageService.GetAllEntitiesAsync<Customer>();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            // Generate CustomerID if not provided
            if (string.IsNullOrEmpty(customer.CustomerID))
            {
                customer.CustomerID = Guid.NewGuid().ToString();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _storageService.AddEntityAsync(customer);
                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error creating customer: {ex.Message}");
                }
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            _logger.LogInformation($"Edit action called with id: {id}");

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit called with null or empty id");
                return NotFound();
            }

            try
            {
                var customer = await _storageService.GetEntityAsync<Customer>("Customer", id);

                if (customer == null)
                {
                    _logger.LogWarning($"Customer with id {id} not found");
                    return NotFound();
                }

                _logger.LogInformation($"Successfully retrieved customer: {customer.RowKey}");
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving customer with id {id}");
                return StatusCode(500, "An error occurred while retrieving the customer");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            _logger.LogInformation("Edit action started");
            _logger.LogInformation($"Customer RowKey: {customer.RowKey}");
            _logger.LogInformation($"Customer PartitionKey: {customer.PartitionKey}");
            _logger.LogInformation($"Customer ETag: {customer.ETag}");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

            if (!ModelState.IsValid)
            {
                // Log validation errors
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
                return View(customer);
            }

            try
            {
                _logger.LogInformation("Attempting to get original customer");
                var originalCustomer = await _storageService.GetEntityAsync<Customer>("Customer", customer.RowKey);

                if (originalCustomer == null)
                {
                    _logger.LogWarning($"Customer not found: {customer.RowKey}");
                    return NotFound();
                }

                _logger.LogInformation("Original customer retrieved successfully");

                // Update properties
                originalCustomer.FirstName = customer.FirstName;
                originalCustomer.LastName = customer.LastName;
                originalCustomer.Username = customer.Username;
                originalCustomer.Email = customer.Email;
                originalCustomer.ShippingAddress = customer.ShippingAddress;

                _logger.LogInformation("Properties updated, attempting to save to storage");
                await _storageService.UpdateEntityAsync(originalCustomer);
                _logger.LogInformation("Customer updated successfully");

                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (RequestFailedException ex) when (ex.Status == 412)
            {
                _logger.LogWarning(ex, "ETag mismatch during customer update");
                ModelState.AddModelError("", "The customer was modified by another user. Please reload and try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
            }

            _logger.LogInformation("Returning to Edit view");
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _storageService.DeleteEntityAsync<Customer>("Customer", id);
                TempData["Success"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}