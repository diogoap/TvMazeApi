using Repository;
using ScrapperWorker.Clients;
using System.Diagnostics;
using Show = ScrapperWorker.Models.Show;

namespace ScrapperWorker.Services
{
    public class TvShowScrapperService : ITvShowScrapperService
    {
        private readonly ILogger<TvShowScrapperService> _logger;
        private readonly ITvMazeApiClient _tvMazeApiClient;
        private readonly IShowRepository _showRepository;
        private readonly IShowPageRepository _showPageRepository;

        public TvShowScrapperService(ILogger<TvShowScrapperService> logger, ITvMazeApiClient tvMazeApiClient, IShowRepository showRepository, IShowPageRepository showPageRepository)
        {
            _logger = logger;
            _tvMazeApiClient = tvMazeApiClient;
            _showRepository = showRepository;
            _showPageRepository = showPageRepository;
        }

        public async Task LoadShows()
        {
            var continueSearch = true;
            
            while (continueSearch)
            {
                var sw = Stopwatch.StartNew();

                var showPageNumber = _showPageRepository.GetLastStoredPage() + 1;

                _logger.LogWarning($"Loading Show page number {showPageNumber} from Api.");
                continueSearch = await LoadShowsByPageNumber(showPageNumber);

                if (!continueSearch)
                {
                    _logger.LogWarning($"Show page number {showPageNumber} NOT found. Stopping search.");
                    break;
                }

                sw.Stop();

                _logger.LogWarning($"Page number {showPageNumber} processed within {sw.ElapsedMilliseconds}ms");
            }
        }

        public async Task<bool> LoadShowsByPageNumber(int showPageNumber)
        {
            var loadShowsResult = await _tvMazeApiClient.LoadShowsFromTvMazeApiByPageNumber(showPageNumber);

            if (loadShowsResult == null)
            {
                _logger.LogDebug($"Show page number {showPageNumber} did not return results.");
                return await ValueTask.FromResult(false);
            }

            foreach (var show in loadShowsResult)
            {
                if (_showRepository.GetShow(show.Id) != null)
                {
                    _logger.LogDebug($"Show with ID {show.Id} already exists. Skipping...");
                    continue;
                }

                var cast = await _tvMazeApiClient.LoadCastFromTvMazeApi(show.Id);

                _logger.LogDebug($"ShowDB: Adding show ID - {show.Id} - Name: {show.Name} - Cast count: {show.Cast?.Count()}");
                _showRepository.AddShow(Show.MapToDbModel(show, cast));
            }

            _logger.LogDebug($"ShowDB: Adding show page number: {showPageNumber}");
            _showPageRepository.AddShowPage(showPageNumber);
            return await ValueTask.FromResult(true);
        }
    }
}
