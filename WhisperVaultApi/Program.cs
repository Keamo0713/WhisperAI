using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using WhisperVaultApi.Data;
using WhisperVaultApi.Services;
using Microsoft.Identity.Web;              // Add this for Azure AD
using Microsoft.AspNetCore.Authentication.OpenIdConnect;  // Add this for OpenID Connect

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllersWithViews();

// Add DbContext (Azure SQL or local dev)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()
    ));


// Register background service
builder.Services.AddHostedService<ConfessionCleanupService>();

// Add Azure AD Authentication
//builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
  //  .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

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

// Add authentication & authorization middleware here:
//app.UseAuthentication();
//app.UseAuthorization();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
