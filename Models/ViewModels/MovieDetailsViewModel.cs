namespace Spring2026_CS330_Project3_rhhunter.Models.ViewModels
{
    public class MovieDetailsViewModel
    {
        public required Movie Movie { get; set; }
        public required IEnumerable<Actor> Actors { get; set; }

        public List<ReviewContainer> Reviews { get; set; }
        public double AverageSentiment { get; set; }

        //labels put a word to the numbers, and the class will let us color the text to match
        public string AverageSentimentLabel => AverageSentiment >= 0.05 ? "Positive"
                                             : AverageSentiment <= -0.05 ? "Negative"
                                             : "Neutral";
        public string AverageSentimentClass => AverageSentiment >= 0.05 ? "text-success"
                                             : AverageSentiment <= -0.05 ? "text-danger"
                                             : "text-muted";
    }

    public class ReviewContainer
    {
        public string Review { get; set; }
        public double Score { get; set; }

        //labels put a word to the numbers, and the class will let us color the text to match
        public string Label => Score >= 0.05 ? "Positive"
                             : Score <= -0.05 ? "Negative"
                             : "Neutral";
        public string LabelClass => Score >= 0.05 ? "text-success"
                                  : Score <= -0.05 ? "text-danger"
                                  : "text-muted";

    }
}
