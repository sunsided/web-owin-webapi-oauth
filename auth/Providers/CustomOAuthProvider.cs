using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

namespace auth.Providers
{
    /// <summary>
    /// Provider für OAuth-Token
    /// </summary>
    public class CustomOAuthProvider : OAuthAuthorizationServerProvider
    {

        /// <summary>
        /// Diese Methode validiert, ob der Ressourcenserver, der ein Token von uns anfordert,
        /// überhaupt im <see cref="Entities.Audience"/>-Repository hinterlegt ist.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        [NotNull]
        public override Task ValidateClientAuthentication([NotNull] OAuthValidateClientAuthenticationContext context)
        {
            Console.WriteLine("Validating Client Authentication ...");

            // Client-ID und Client-Secret aus dem Kontext beziehen
            string clientId;
            string clientSecret;
            if (!context.TryGetBasicCredentials(out clientId, out clientSecret))
            {
                context.TryGetFormCredentials(out clientId, out clientSecret);
            }

            // Wir erwarten eine gesetzte Client-ID
            if (context.ClientId == null)
            {
                context.SetError("invalid_clientId", "client_id is not set");
                return Task.FromResult<object>(null);
            }

            // Wir erwarten ebenfalls, dass der Client registriert ist
            var audience = AudiencesStore.FindAudience(context.ClientId); // TODO ... async
            if (audience == null)
            {
                context.SetError("invalid_clientId", $"client_id '{context.ClientId}' is invalid");
                return Task.FromResult<object>(null);
            }

            // An dieser Stelle ist der Request gültig.
            context.Validated();
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Diese Methode validiert, ob die Angaben des Ressourcenbesitzers gültig sind.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>Task.</returns>
        [NotNull]
        public override Task GrantResourceOwnerCredentials([NotNull] OAuthGrantResourceOwnerCredentialsContext context)
        {
            Console.WriteLine("Grating Resource Owner credentials ...");

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            // Dummy check here, you need to do your DB checks against membership system http://bit.ly/SPAAuthCode
            if (context.UserName != "chucknorris" || context.Password != "geheim")
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
                
		Console.WriteLine("Credentials are invalid.");
                return Task.FromResult<object>(null);
            }

            // Da die Daten des Ressourcenbesitzers gültig sind, stellen wir ein neues Token aus.
            var identity = new ClaimsIdentity("JWT");

            identity.AddClaim(new Claim(ClaimTypes.Name, context.UserName));
            identity.AddClaim(new Claim("sub", context.UserName));

            // Rollen hinzufügen
            // TODO: Aus angeforderten Rollen beziehen
            identity.AddClaim(new Claim(ClaimTypes.Role, "Manager"));
            identity.AddClaim(new Claim(ClaimTypes.Role, "Supervisor"));

            var props = new AuthenticationProperties(
                new Dictionary<string, string>
                {
                    {
                         "audience", context.ClientId ?? string.Empty // TODO: Die ClientID sollte niemals null sein -- asserten!
                    }
                });

            // Ticket erstellen und ab dafür
            Console.WriteLine("Creating the ticket ...");

            var ticket = new AuthenticationTicket(identity, props);
            context.Validated(ticket);

            Console.WriteLine("All is well!");
            return Task.FromResult<object>(null);
        }
    }
}
