namespace ScrapperWorker.Models
{
    public class Show
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Cast>? Cast { get; set; }

        public static Domain.Models.Show MapToDbModel(Show show, IEnumerable<Cast>? cast)
        {
            return new Domain.Models.Show
            {
                Id = show.Id,
                Name = show.Name,
                Cast = cast?.Select(c => new Domain.Models.Cast
                {
                    Id = c.Person.Id,
                    Name = c.Person.Name,
                    Birthday = DateTime.TryParse(c.Person.Birthday, out var date) ? date.Date : null,
                }),
            };
        }
    }
}