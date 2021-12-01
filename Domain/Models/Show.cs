namespace Domain.Models
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Cast>? Cast { get; set; }
    }
}