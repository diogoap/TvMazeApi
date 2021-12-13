namespace ScrapperWorker.Services
{
    public interface ITvShowScrapperService
    {
        Task LoadShows(CancellationToken cancellationToken);
        Task<bool> LoadShowsByPageNumber(int showPageNumber, CancellationToken cancellationToken);
    }
}