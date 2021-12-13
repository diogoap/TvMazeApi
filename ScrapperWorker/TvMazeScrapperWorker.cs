using ScrapperWorker.Services;

namespace ScrapperWorker
{
    public class TvMazeScrapperWorker : BackgroundService
    {
        private readonly ILogger<TvMazeScrapperWorker> _logger;
        private readonly ITvShowScrapperService _tvShowScrapper;

        public TvMazeScrapperWorker(ILogger<TvMazeScrapperWorker> logger, ITvShowScrapperService tvShowScrapper)
        {
            _logger = logger;
            _tvShowScrapper = tvShowScrapper;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await _tvShowScrapper.LoadShows(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}