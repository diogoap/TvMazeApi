using Domain.Models;

namespace Repository
{
    public interface IShowRepository
    {
        void AddShow(Show show);
        IEnumerable<Show> GetShows(int pageNumber, int pageSize);
        Show? GetShow(int id);
    }
}