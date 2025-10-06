using cloud1.Models;
using cloud1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace cloud1.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(IFunctionsApi functionsApi, ILogger<CustomerController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation("Retrieving all customers");
                var customers = await _functionsApi.GetCustomersAsync();
                _logger.LogInformation($"Retrieved {customers?.Count ?? 0} customers");
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers");
                TempData["Error"] = "Error retrieving customers. Please try again later.";
                return View(new List<Customer>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            _logger.LogInformation("Create POST action started");
            _logger.LogInformation($"ModelState.IsValid: {ModelState.IsValid}");

            // Log validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    _logger.LogWarning($"Validation error: {error.ErrorMessage}");
                }
            }

            // Generate CustomerID if not provided
            if (string.IsNullOrEmpty(customer.CustomerID))
            {
                customer.CustomerID = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                _logger.LogInformation($"Generated CustomerID: {customer.CustomerID}");
            }

            // Set RowKey to match CustomerID for Table Storage
            customer.RowKey = customer.CustomerID;

            if (ModelState.IsValid)
            {
                try
                {
                    _logger.LogInformation("Attempting to create customer via API");
                    _logger.LogInformation($"Customer data - ID: {customer.CustomerID}, Name: {customer.FirstName} {customer.LastName}, Email: {customer.Email}");

                    await _functionsApi.CreateCustomerAsync(customer);
                    _logger.LogInformation("Customer created successfully via API");

                    TempData["Success"] = "Customer created successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating customer");
                    ModelState.AddModelError("", $"Error creating customer: {ex.Message}");

                    // Log the customer data that failed to create
                    _logger.LogError($"Failed customer data - ID: {customer.CustomerID}, Name: {customer.FirstName} {customer.LastName}, Email: {customer.Email}");
                }
            }

            _logger.LogWarning("Returning to Create view with validation errors");
            return View(customer);
        }

        public async Task<IActionResult> Edit(string id)
        {
            _logger.LogInformation($"Edit GET action called with id: {id}");

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit called with null or empty id");
                TempData["Error"] = "Customer ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var customer = await _functionsApi.GetCustomerAsync(id);

                if (customer == null)
                {
                    _logger.LogWarning($"Customer with id {id} not found");
                    TempData["Error"] = "Customer not found";
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Successfully retrieved customer: {customer.CustomerID}");
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving customer with id {id}");
                TempData["Error"] = "Error retrieving customer details";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            _logger.LogInformation("Edit POST action started");
            _logger.LogInformation($"Customer ID: {customer.CustomerID}");
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
                _logger.LogInformation("Attempting to update customer");
                _logger.LogInformation($"Updating customer - ID: {customer.CustomerID}, Name: {customer.FirstName} {customer.LastName}");

                await _functionsApi.UpdateCustomerAsync(customer.CustomerID, customer);
                _logger.LogInformation("Customer updated successfully");

                TempData["Success"] = "Customer updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer: {Message}", ex.Message);
                ModelState.AddModelError("", $"Error updating customer: {ex.Message}");
            }

            _logger.LogInformation("Returning to Edit view");
            return View(customer);
        }

        public async Task<IActionResult> Details(string id)
        {
            _logger.LogInformation($"Details action called with id: {id}");

            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Customer ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var customer = await _functionsApi.GetCustomerAsync(id);

                if (customer == null)
                {
                    TempData["Error"] = "Customer not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving customer details with id {id}");
                TempData["Error"] = "Error retrieving customer details";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            _logger.LogInformation($"Delete action called with id: {id}");

            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Customer ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _functionsApi.DeleteCustomerAsync(id);
                _logger.LogInformation($"Customer with ID {id} deleted successfully");
                TempData["Success"] = "Customer deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer with ID: {Id}", id);
                TempData["Error"] = $"Error deleting customer: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}