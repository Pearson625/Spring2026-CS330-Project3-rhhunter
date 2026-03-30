using Spring2026_CS330_Project3_rhhunter.Models;

namespace Spring2026_CS330_Project3_rhhunter.Models.ViewModels
{
    public class ActorDetailsViewModel
    {
        public required Actor Actor { get; init; }
        public required IEnumerable<Movie> Movies { get; init; }
        public List<TweetContainer> Tweets { get; init; }
        public double AverageSentiment { get; init; }
        public string AverageSentimentLabel => AverageSentiment >= 0.05 ? "Positive"
                                             : AverageSentiment <= -0.05 ? "Negative"
                                             : "Neutral";
        public string AverageSentimentClass => AverageSentiment >= 0.05 ? "text-success"
                                             : AverageSentiment <= -0.05 ? "text-danger"
                                             : "text-muted";
    }


    public class TweetContainer
    {
        public string Tweet { get; init; }
        public double Score { get; init; }
        public string Label => Score >= 0.05 ? "Positive"
                             : Score <= -0.05 ? "Negative"
                             : "Neutral";
        public string LabelClass => Score >= 0.05 ? "text-success"
                                  : Score <= -0.05 ? "text-danger"
                                  : "text-muted";
    }
}