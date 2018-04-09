using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SQLCompare.UI.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "Pluto";
        }
    }
}