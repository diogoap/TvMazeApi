using LiteDB;
using Polly;
using Repository;
using ScrapperWorker;
using ScrapperWorker.Clients;
using ScrapperWorker.Services;
using System.Net;

IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
configurationBuilder.AddJsonFile("appsettings.json");
IConfiguration configuration = configurationBuilder.Build();

var retryPolice =
  Policy
    .Handle<HttpRequestException>()
    .OrResult<HttpResponseMessage>(r => r.StatusCode == HttpStatusCode.TooManyRequests)
    .WaitAndRetryAsync(
        retryCount: 3,
        sleepDurationProvider: (retryCount) =>
        {
            return TimeSpan.FromSeconds(10 * retryCount);
        });

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddTransient<IShowRepository, ShowRepository>();
        services.AddTransient<IShowPageRepository, ShowPageRepository>();
        services.AddSingleton<ILiteRepository>(x => new LiteRepository(configuration["ShowsDbConnectionString"]));
        services.AddTransient<ITvShowScrapperService, TvShowScrapperService>();
        services.AddHostedService<TvMazeScrapperWorker>();
        services.AddHttpClient<ITvMazeApiClient, TvMazeApiClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["TvMazeApiBaseUrl"]);
        }).AddPolicyHandler(retryPolice);
    })
    .Build();

await host.RunAsync();
