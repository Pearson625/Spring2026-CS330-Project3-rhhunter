namespace Spring2026_CS330_Project3_rhhunter.Models.ViewModels
{
    public class ActorDetailsViewModel
    {
        public required Actor Actor { get; init; }
        public required IEnumerable<Movie> Movies { get; init; }
        
    }
}
