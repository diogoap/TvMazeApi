namespace ScrapperWorker.Services
{
    public interface ITvShowScrapperService
    {
        Task LoadShows();
        Task<bool> LoadShowsByPageNumber(int showPageNumber);
    }
}