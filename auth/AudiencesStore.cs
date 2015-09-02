using System;
using System.Collections.Concurrent;
using System.Security.Cryptography;
using auth.Entities;
using JetBrains.Annotations;
using Microsoft.Owin.Security.DataHandler.Encoder;

namespace auth
{
    /// <summary>
    /// Repository for <see cref="Audience"/> resources.
    /// </summary>
    public static class AudiencesStore // TODO: Sollte nicht static sein
    {
        /// <summary>
        /// Naive Implementierung
        /// TODO: Datenbankzugriffe implementieren
        /// </summary>
        private static readonly ConcurrentDictionary<string, Audience> AudiencesList = new ConcurrentDictionary<string, Audience>();

        /// <summary>
        /// Initializes static members of the <see cref="AudiencesStore"/> class.
        /// </summary>
        static AudiencesStore()
        {
            // Demo-Audience hinzufügen
            var sampleAudienceId = "099153c2625149bc8ecb3e85e03f0022";
            AudiencesList.TryAdd(
                sampleAudienceId,
                new Audience
                    {
                        ClientId = sampleAudienceId,
                        Base64Secret = "IxrAjDoa2FqElO7IhrSrUJELhUckePEPVpaePlS_Xaw",
                        Name = "ResourceServer.Api 1"
                    });
        }

        /// <summary>
        /// Registriert eine Audience.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>Die neu erstellte <see cref="Audience"/>.</returns>
        /// <exception cref="OverflowException">The internal dictionary already contains the maximum number of elements (<see cref="F:System.Int32.MaxValue" />).</exception>
        [NotNull]
        public static Audience AddAudience([NotNull] string name) // TODO: Sollte nicht static sein
        {
            var clientId = Guid.NewGuid().ToString("N");

            var key = new byte[32];
            RNGCryptoServiceProvider.Create().GetBytes(key);
            var base64Secret = TextEncodings.Base64Url.Encode(key);

            var newAudience = new Audience { ClientId = clientId, Base64Secret = base64Secret, Name = name };
            AudiencesList.TryAdd(clientId, newAudience);

            return newAudience;
        }

        /// <summary>
        /// Ermittelt eine Audience anhand ihrer <see cref="clientId" />.
        /// </summary>
        /// <param name="clientId">The client identifier.</param>
        /// <returns>Die <see cref="Audience"/> oder <see langword="null"/>, falls kein passender Eintrag existierte.</returns>
        [CanBeNull]
        public static Audience FindAudience([NotNull] string clientId) // TODO: Sollte nicht static sein
        {
            Audience audience;
            return AudiencesList.TryGetValue(clientId, out audience)
                ? audience
                : null;
        }
    }
}
