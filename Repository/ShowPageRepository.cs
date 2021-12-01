using Domain.Models;
using LiteDB;

namespace Repository
{
    public class ShowPageRepository : IShowPageRepository
    {
        private readonly ILiteRepository _db;
        public ShowPageRepository(ILiteRepository liteRepository)
        {
            _db = liteRepository;
        }

        public void AddShowPage(int pageNumber)
        {
            _db.Insert(new ShowPage { Id = pageNumber } );
        }

        public int GetLastStoredPage()
        {
            var page = _db.Query<ShowPage>().OrderByDescending(o => o.Id).FirstOrDefault();
            return page != null ? page.Id : 0;
        }
    }
}