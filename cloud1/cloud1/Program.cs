using System.Globalization;
using cloud1.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

// Culture (for decimal handling)
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Add configuration (appsettings.json)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// MVC with API Controllers - Register all services BEFORE building
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); // For API controllers

// Typed HttpClient for your Azure Functions/API
builder.Services.AddHttpClient("Functions", (sp, client) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();

    // Try to get from config, fallback to localhost for development
    var baseUrl = cfg["Functions:BaseUrl"];

    if (string.IsNullOrEmpty(baseUrl))
    {
        // Development fallback - use current app's own API
        baseUrl = "https://cldvpoefunction-ctegdnabe3ambzcr.canadacentral-01.azurewebsites.net"; // Change this to your actual port
    }

    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
    client.Timeout = TimeSpan.FromSeconds(100);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

// Register the FunctionsApiClient
builder.Services.AddScoped<IFunctionsApi, FunctionsApiClient>();

// Allow larger multipart uploads (images, proofs, etc.)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024; // 50 MB
});

// Logging
builder.Services.AddLogging();

// Build the application - AFTER this point, builder.Services becomes read-only
var app = builder.Build();

// Pipeline configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage(); // Better error messages in development
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Map both MVC and API routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapControllers(); // For API controllers

app.Run();