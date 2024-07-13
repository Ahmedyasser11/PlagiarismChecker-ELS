using System.ComponentModel.DataAnnotations;

namespace PlagiarismChecker.Models
{
    public class Content
    {
        [Required]
        public string s1 { get; set; }
        [Required]
        public string s2 { get; set; }
    }
}
