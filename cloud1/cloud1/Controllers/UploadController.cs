// Controllers/UploadController.cs
using Microsoft.AspNetCore.Mvc;
using cloud1.Models;
using cloud1.Services;
using Microsoft.Extensions.Logging;

namespace cloud1.Controllers
{
    public class UploadController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IFunctionsApi functionsApi, ILogger<UploadController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public ActionResult Index()
        {
            return View(new FileUploadModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (model.ProofOfPayment != null && model.ProofOfPayment.Length > 0)
                    {
                        // Upload using the Functions API
                        var fileName = await _functionsApi.UploadProofOfPaymentAsync(
                            model.ProofOfPayment,
                            model.OrderId,
                            model.CustomerName
                        );

                        _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
                        TempData["Success"] = $"File uploaded successfully! File name: {fileName}";

                        // Clear the model for a fresh form
                        return View(new FileUploadModel());
                    }
                    else
                    {
                        ModelState.AddModelError("ProofOfPayment", "Please select a file to upload.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error uploading file");
                    ModelState.AddModelError("", $"Error uploading file: {ex.Message}");
                }
            }
            return View(model);
        }
    }
}