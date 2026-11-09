using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
var culture = CultureInfo.InvariantCulture;

CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// API connection
builder.Services.AddHttpClient("RaceDayApi", client =>
{
    var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
                 ?? "http://localhost:5290/";

    client.BaseAddress = new Uri(apiUrl);
});

// Session configuration
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// MVC services
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
