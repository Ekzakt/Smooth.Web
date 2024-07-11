namespace Smooth.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                options.Authority = builder.Configuration.GetValue<string>("IdentityServer:Authority");
                options.ClientId = builder.Configuration.GetValue<string>("IdentityServer:ClientId");
                options.ClientSecret = builder.Configuration.GetValue<string>("IdentityServer:ClientSecret");
                options.ResponseType = "code";
                
                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("flauntapi.read");
                
                options.MapInboundClaims = false; // Don't rename claim types

                options.SaveTokens = true;
            });

            builder.Services.AddRazorPages();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
