using System.ComponentModel.DataAnnotations;

namespace Spring2026_CS330_Project3_rhhunter.Models
{
    public class Actor
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Gender { get; set; } = "";

        public int Age {  get; set; }

        public string IMDB {  get; set; } = "";

        public byte[]? Photo { get; set; }

    }
}
