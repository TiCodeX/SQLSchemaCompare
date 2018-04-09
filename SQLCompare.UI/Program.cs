using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SQLCompare.UI
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseUrls("http://0.0.0.0:5000")
                .UseStartup<Startup>()
                .Build();
    }
}
