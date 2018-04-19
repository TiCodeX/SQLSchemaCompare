using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace SQLCompare.UI.Pages
{
    public class IndexModel : PageModel
    {
        private ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }
        public void OnGet()
        {
            ViewData["Title"] = "Pluto";
            _logger.LogError("This is an error!");
        }
    }
}