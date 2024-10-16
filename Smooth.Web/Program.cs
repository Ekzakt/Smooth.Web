using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Smooth.Web.Data;

namespace Smooth.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var environment = builder.Environment;

        IdentityModelEventSource.ShowPII = environment.IsDevelopment();

        // 1. Configure Forwarded Headers Middleware
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        // 2. Configure Route Options
        builder.Services.Configure<RouteOptions>(routeOptions =>
        {
            routeOptions.LowercaseUrls = true;
        });

        // 3. Add Razor Pages
        builder.Services.AddRazorPages();

        // 4. Configure Database Context
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionString")));

        // 5. Configure Data Protection
        builder.Services.AddDataProtection()
                .PersistKeysToDbContext<ApplicationDbContext>()
                .SetApplicationName("SmoothSensation.SharedCookie");

        // 6. Configure HTTPS Redirection Options
        builder.Services.Configure<HttpsRedirectionOptions>(options =>
        {
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
            options.HttpsPort = 443; // Explicitly set HTTPS port
        });

        // 7. Configure Authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.Cookie.Name = ".AspNet.SharedCookie";
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            options.Cookie.Domain = builder.Configuration["IdentityServer:CookieDomain"];
            options.Cookie.HttpOnly = true;
            options.Cookie.Path = "/";
            options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Increased from 1 minute
            options.SlidingExpiration = true;
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = builder.Configuration["IdentityServer:Authority"];
            options.ClientId = builder.Configuration["IdentityServer:ClientId"];
            options.ClientSecret = builder.Configuration["IdentityServer:ClientSecret"];
            options.RequireHttpsMetadata = !environment.IsDevelopment();
            options.SaveTokens = true;
            options.ResponseType = "code";
            options.ResponseMode = "query";
            options.GetClaimsFromUserInfoEndpoint = true;
            options.UsePkce = true;
            options.MapInboundClaims = false;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("offline_access");
            options.Scope.Add("flauntapi.read");

            // Optional: Add event handlers for debugging
            options.Events = new OpenIdConnectEvents
            {
                OnAuthenticationFailed = context =>
                {
                    context.HandleResponse();
                    context.Response.Redirect("/Error?message=" + Uri.EscapeDataString(context.Exception.Message));
                    return Task.CompletedTask;
                }
            };
        });

        var app = builder.Build();

        // 8. Use Forwarded Headers Middleware
        app.UseForwardedHeaders();

        // 9. Exception Handling and HSTS
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        // 10. Use HTTPS Redirection Middleware
        app.UseHttpsRedirection();

        // 11. Use Static Files
        app.UseStaticFiles();

        // 12. Use Routing
        app.UseRouting();

        // 13. Use Authentication and Authorization
        app.UseAuthentication(); // Added
        app.UseAuthorization();  // Single call

        // 14. Map Razor Pages
        app.MapRazorPages();

        app.Run();
    }
}
