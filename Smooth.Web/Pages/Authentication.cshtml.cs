using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Smooth.Web.Pages;

[Authorize]
public class AuthenticationModel : PageModel
{
    [BindProperty]
    public AuthenticationProperties? AuthenticationProperties { get; set; }

    public async Task OnGet()
    {
        var result = await HttpContext.AuthenticateAsync();

        if (result.Succeeded)
        {
            AuthenticationProperties = result.Properties;
        }
    }
}
