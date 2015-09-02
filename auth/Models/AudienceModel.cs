using System.ComponentModel.DataAnnotations;

namespace auth.Models
{
    public class AudienceModel
    {
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
    }
}
