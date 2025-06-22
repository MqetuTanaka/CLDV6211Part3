using Microsoft.EntityFrameworkCore;
using Web.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();



// Register the DbContext with dependency injection
builder.Services.AddDbContext<WebdevP3Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "about",
    pattern: "about-us",
    defaults: new { controller = "AboutUs", action = "Index" });

app.MapControllerRoute(
    name: "booking",
    pattern: "booking",
    defaults: new { controller = "Booking", action = "Index" }
);

app.MapControllerRoute(
    name: "event",
    pattern: "event",
    defaults: new { controller = "Event", action = "Index" }
);

app.MapControllerRoute(
    name: "venue",
    pattern: "venue",
    defaults: new { controller = "Venue", action = "Index" }
);
app.Run();