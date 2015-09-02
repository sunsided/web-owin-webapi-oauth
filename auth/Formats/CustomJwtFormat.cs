using System;
using System.IdentityModel.Tokens;
using JetBrains.Annotations;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Thinktecture.IdentityModel.Tokens;

namespace auth.Formats
{
    /// <summary>
    /// Bentuzerdefiniertes Format für die JSON Web Token
    /// </summary>
    public class CustomJwtFormat : ISecureDataFormat<AuthenticationTicket>
    {
        /// <summary>
        /// Key des Audience-Wertes im <see cref="AuthenticationProperties.Dictionary"/> der <see cref="AuthenticationTicket.Properties"/>.
        /// </summary>
        private const string AudiencePropertyKey = "audience";

        /// <summary>
        /// Die ID des Token-Issuers
        /// </summary>
        [NotNull]
        private readonly string _issuer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomJwtFormat"/> class.
        /// </summary>
        /// <param name="issuer">The issuer.</param>
        public CustomJwtFormat([NotNull] string issuer)
        {
            _issuer = issuer;
        }

        /// <summary>
        /// Erzeugt das JWT.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>System.String.</returns>
        /// <exception cref="System.ArgumentNullException">data</exception>
        /// <exception cref="System.InvalidOperationException">AuthenticationTicket.Properties does not include audience</exception>
        [NotNull]
        public string Protect([CanBeNull] AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            // Audience ermitteln; dies entspricht der ClientID des Ressourcenproviders
            string audienceId = data.Properties.Dictionary.ContainsKey(AudiencePropertyKey)
                ? data.Properties.Dictionary[AudiencePropertyKey]
                : null;

            // Wir erwarten, dass die ID gesetzt ist
            if (string.IsNullOrWhiteSpace(audienceId))
            {
                throw new InvalidOperationException("AuthenticationTicket.Properties does not include audience");
            }

            // Audience anhand ihrer ID ermitteln
            var audience = AudiencesStore.FindAudience(audienceId);
            if (audience == null)
            {
                throw new InvalidOperationException("Audience's ClientID is invalid.");
            }

            // Symmetrischen Key aus Audience ermitteln
            var symmetricKeyAsBase64 = audience.Base64Secret;
            var keyByteArray = TextEncodings.Base64Url.Decode(symmetricKeyAsBase64);

            // Signierungscredentials erzeugen
            var signingKey = new HmacSigningCredentials(keyByteArray);

            // Timestamps beziehen
            var issued = data.Properties.IssuedUtc?.UtcDateTime;
            var expires = data.Properties.ExpiresUtc?.UtcDateTime;

            // Token erzeugen und erzeugen
            var token = new JwtSecurityToken(_issuer, audienceId, data.Identity.Claims, issued, expires, signingKey);

            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        /// <summary>
        /// Unprotect wird hier nicht benötigt.
        /// </summary>
        /// <param name="protectedText">The protected text.</param>
        /// <returns>Microsoft.Owin.Security.AuthenticationTicket.</returns>
        [NotNull]
        public AuthenticationTicket Unprotect([CanBeNull] string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}
