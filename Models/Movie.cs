using System.ComponentModel.DataAnnotations;

namespace Spring2026_CS330_Project3_rhhunter.Models
{
    public class Movie
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; } = "";

        public string IMDB { get; set; } = "";

        public string Genre {  get; set; } = "";

        public int ReleaseYear { get; set; }

        public byte[]? Poster {  get; set; } 


    }
}
