namespace Repository
{
    public interface IShowPageRepository
    {
        void AddShowPage(int pageNumber);
        int GetLastStoredPage();
    }
}