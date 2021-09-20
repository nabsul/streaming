using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "generate")
            {
                DataGenerator.GenerateData();
                return;
            }

            WebHost
                .CreateDefaultBuilder()
                .ConfigureServices(svc => svc.AddControllers())
                .UseKestrel(o => o.Listen(IPAddress.Any, 5000))
                .Configure(app => app.UseRouting().UseEndpoints(e => e.MapControllers()))
                .Build().Run();
        }

    }
}
