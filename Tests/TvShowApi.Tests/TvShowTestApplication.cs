using LiteDB;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace TvShowApi.Tests
{
    internal class TvShowTestApplication : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ILiteRepository>(x => new LiteRepository(new MemoryStream()));
            });

            return base.CreateHost(builder);
        }
    }
}
