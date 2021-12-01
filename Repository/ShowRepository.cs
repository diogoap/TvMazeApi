using Domain.Models;
using LiteDB;

namespace Repository
{
    public class ShowRepository : IShowRepository
    {
        private readonly ILiteRepository _db;

        public ShowRepository(ILiteRepository liteRepository)
        {
            _db = liteRepository;
        }

        public void AddShow(Show show)
        {
            _db.Insert(show);
        }

        public IEnumerable<Show> GetShows()
        {   
            return _db.Query<Show>().ToEnumerable();
        }

        public IEnumerable<Show> GetShows(int pageNumber, int pageSize)
        {
            var showList = _db.Query<Show>().Limit(pageSize).Offset((pageNumber - 1) * pageSize).ToEnumerable();

            return showList.Select(s =>
            { 
                s.Cast = s.Cast?.OrderByDescending(s => s.Birthday);
                return s;
            });
        }

        public Show? GetShow(int id)
        {
            return _db.SingleOrDefault<Show>(x => x.Id == id);
        }
    }
}