using System.ComponentModel.DataAnnotations;

namespace auth.Entities
{
    /// <summary>
    /// Audiences sind Ressourcenserver, die von uns ein JWT-Token anfordern dürfen.
    /// </summary>
    public class Audience
    {
        /// <summary>
        /// Bezieht oder setzt die ID der Audience.
        /// </summary>
        [Key]
        [MaxLength(32)]
        public string ClientId { get; set; }

        /// <summary>
        /// Bezieht oder setzt das Secret der Audience.
        /// </summary>
        /// <remarks>
        /// Dieses Secret ist nur dem Authorization Server und der
        /// entsprechenden Audience (dem Resource Server) bekannt.
        /// </remarks>
        [MaxLength(80)]
        [Required]
        public string Base64Secret { get; set; }

        /// <summary>
        /// Bezieht oder setzt den lesbaren Namen der Audience.
        /// </summary>
        [MaxLength(100)]
        [Required]
        public string Name { get; set; }
    }
}
