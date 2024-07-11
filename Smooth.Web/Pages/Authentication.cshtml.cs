using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Smooth.Web.Pages;

[Authorize]
public class AuthenticationModel : PageModel
{
    public void OnGet()
    {
    }
}
