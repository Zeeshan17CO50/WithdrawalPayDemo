using Microsoft.EntityFrameworkCore;
using WithdrawalApp.Data;
using WithdrawalApp.Interfaces;
using WithdrawalApp.Services;
using NLog.Web;
using System.Reflection.Metadata;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
logger.Debug("Init main");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IWalletService, WalletService>();

// Add NLog
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// HttpClient for PaymentSimulator (base address from configuration)
builder.Services.AddHttpClient();

var app = builder.Build();

// Ensure DB migrated and seeded
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers(); 
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Wallet}/{action=Index}/{id?}"); // For Razor Views
});

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Wallet}/{action=Index}/{id?}");

app.Run();
