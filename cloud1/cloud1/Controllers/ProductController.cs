// Controllers/ProductController.cs
using cloud1.Models;
using cloud1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;

namespace cloud1.Controllers
{
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi functionsApi, ILogger<ProductController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionsApi.GetProductsAsync();
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                TempData["Error"] = "Error retrieving products. Please try again later.";
                return View(new List<Product>());
            }
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            // Log the price for debugging
            _logger.LogInformation("Product price submitted: {Price}", product.Price);

            if (ModelState.IsValid)
            {
                try
                {
                    // Validate price is greater than $8.00 as per business rule
                    if (product.Price < 8.00)
                    {
                        ModelState.AddModelError("Price", "Price must be greater than $8.00");
                        return View(product);
                    }

                    // Create product with image if provided
                    var newProduct = await _functionsApi.CreateProductAsync(product, imageFile);

                    TempData["Success"] = $"Product '{newProduct.ProductName}' created successfully with price {newProduct.Price:C}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                }
            }
            return View(product);
        }

        public async Task<ActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            try
            {
                var product = await _functionsApi.GetProductAsync(id);
                if (product == null)
                {
                    return NotFound();
                }
                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product with ID: {Id}", id);
                TempData["Error"] = "Error retrieving product. Please try again later.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            // Log the price for debugging
            _logger.LogInformation("Edit - Product price submitted: {Price}", product.Price);

            if (ModelState.IsValid)
            {
                try
                {
                    // Update product with image if provided
                    await _functionsApi.UpdateProductAsync(product.RowKey, product, imageFile);

                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product: {Message}", ex.Message);
                    ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                }
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                TempData["Error"] = "Product ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _functionsApi.DeleteProductAsync(id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID: {Id}", id);
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}