namespace Spring2026_CS330_Project3_rhhunter.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public required Movie Movie { get; init; }
        public required IEnumerable<Actor> Actors { get; init; }


    }
}
