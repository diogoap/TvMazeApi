using ScrapperWorker.Models;
using ScrapperWorker.Services;
using System.Net.Http.Json;

namespace ScrapperWorker.Clients
{
    public class TvMazeApiClient : ITvMazeApiClient
    {
        private readonly ILogger<TvShowScrapperService> _logger;
        private readonly HttpClient _httpClient;

        public TvMazeApiClient(ILogger<TvShowScrapperService> logger, HttpClient httpClient)
        {
            _logger = logger;
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Show>?> LoadShowsFromTvMazeApiByPageNumber(int showPageNumber, CancellationToken cancellationToken)
        {
            IEnumerable<Show>? showsPage;
            try
            {
                _logger.LogDebug($"TvMazeApi: Getting shows for page: {showPageNumber}");
                showsPage = await _httpClient.GetFromJsonAsync<IEnumerable<Show>>($"shows?page={showPageNumber}", cancellationToken);
            }
            catch (HttpRequestException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogError($"Getting show page number: {showPageNumber} returned NOT FOUND.");
                    return null;
                }

                _logger.LogError($"HTTP error occurred. Status code: {e.StatusCode} - error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception happended: {e.Message}");
                throw;
            }

            return showsPage;
        }

        public async Task<IEnumerable<Cast>?> LoadCastFromTvMazeApi(int showId, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"TvMazeApi: Getting cast for show ID: {showId}");
                return await _httpClient.GetFromJsonAsync<IEnumerable<Cast>>($"shows/{showId}/cast", cancellationToken);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError($"HTTP error occurred. Status code: {e.StatusCode} - error: {e.Message}");
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError($"Exception happended: {e.Message}");
                throw;
            }
        }
    }
}
