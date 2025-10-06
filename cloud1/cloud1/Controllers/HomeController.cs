// Controllers/HomeController.cs
using cloud1.Models;
using cloud1.Models.ViewModels;
using cloud1.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Framework;
using System.Diagnostics;

namespace cloud1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _functionsApi;

        public HomeController(IFunctionsApi functionsApi)
        {
            _functionsApi = functionsApi;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _functionsApi.GetProductsAsync();
            var customers = await _functionsApi.GetCustomersAsync();
            var orders = await _functionsApi.GetOrdersAsync();

            var viewModel = new HomeViewModel
            {
                FeaturedProducts = products.Take(5).ToList(),
                ProductCount = products.Count,
                CustomerCount = customers.Count,
                OrderCount = orders.Count
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> InitializeStorage()
        {
            try
            {
                // Just test the connection
                await _functionsApi.GetCustomersAsync();
                TempData["Success"] = "API connection initialized successfully!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Failed to initialize API connection: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 8, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}