using ScrapperWorker.Models;

namespace ScrapperWorker.Clients
{
    public interface ITvMazeApiClient
    {
        Task<IEnumerable<Cast>?> LoadCastFromTvMazeApi(int showId, CancellationToken cancellationToken);
        Task<IEnumerable<Show>?> LoadShowsFromTvMazeApiByPageNumber(int showPageNumber, CancellationToken cancellationToken);
    }
}