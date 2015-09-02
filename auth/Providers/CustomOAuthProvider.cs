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
            Console.WriteLine("In ValidateClientAuthentication");

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
            Console.WriteLine("In GrantResourceOwnerCredentials");

            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { "*" });

            // Dummy check here, you need to do your DB checks against membership system http://bit.ly/SPAAuthCode
            if (context.UserName != "chucknorris" || context.Password != "geheim")
            {
                context.SetError("invalid_grant", "The user name or password is incorrect");
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
            return Task.FromResult<object>(null);
        }

        /// <summary>
        /// Called before the AuthorizationEndpoint redirects its response to the caller. The response could be the
        ///             token, when using implicit flow or the AuthorizationEndpoint when using authorization code flow.
        ///             An application may implement this call in order to do any final modification of the claims being used
        ///             to issue access or refresh tokens. This call may also be used in order to add additional
        ///             response parameters to the authorization endpoint's response.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task AuthorizationEndpointResponse(OAuthAuthorizationEndpointResponseContext context)
        {
            Console.WriteLine("In AuthorizationEndpointResponse");
            return base.AuthorizationEndpointResponse(context);
        }

        /// <summary>
        /// Called at the final stage of an incoming Authorize endpoint request before the execution continues on to the web application component
        ///             responsible for producing the html response. Anything present in the OWIN pipeline following the Authorization Server may produce the
        ///             response for the Authorize page. If running on IIS any ASP.NET technology running on the server may produce the response for the
        ///             Authorize page. If the web application wishes to produce the response directly in the AuthorizeEndpoint call it may write to the
        ///             context.Response directly and should call context.RequestCompleted to stop other handlers from executing. If the web application wishes
        ///             to grant the authorization directly in the AuthorizeEndpoint call it cay call context.OwinContext.Authentication.SignIn with the
        ///             appropriate ClaimsIdentity and should call context.RequestCompleted to stop other handlers from executing.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task AuthorizeEndpoint(OAuthAuthorizeEndpointContext context)
        {
            Console.WriteLine("In AuthorizeEndpoint");
            return base.AuthorizeEndpoint(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "authorization_code". This occurs after the Authorize
        ///             endpoint as redirected the user-agent back to the client with a "code" parameter, and the client is exchanging that for an "access_token".
        ///             The claims and properties
        ///             associated with the authorization code are present in the context.Ticket. The application must call context.Validated to instruct the Authorization
        ///             Server middleware to issue an access token based on those claims and properties. The call to context.Validated may be given a different
        ///             AuthenticationTicket or ClaimsIdentity in order to control which information flows from authorization code to access token.
        ///             The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the authorization code to
        ///             the access token unmodified.
        ///             See also http://tools.ietf.org/html/rfc6749#section-4.1.3
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task GrantAuthorizationCode(OAuthGrantAuthorizationCodeContext context)
        {
            Console.WriteLine("In GrantAuthorizationCode");
            return base.GrantAuthorizationCode(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "client_credentials". This occurs when a registered client
        ///             application wishes to acquire an "access_token" to interact with protected resources on it's own behalf, rather than on behalf of an authenticated user.
        ///             If the web application supports the client credentials it may assume the context.ClientId has been validated by the ValidateClientAuthentication call.
        ///             To issue an access token the context.Validated must be called with a new ticket containing the claims about the client application which should be associated
        ///             with the access token. The application should take appropriate measures to ensure that the endpoint isn’t abused by malicious callers.
        ///             The default behavior is to reject this grant type.
        ///             See also http://tools.ietf.org/html/rfc6749#section-4.4.2
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            Console.WriteLine("In GrantClientCredentials");
            return base.GrantClientCredentials(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of any other value. If the application supports custom grant types
        ///             it is entirely responsible for determining if the request should result in an access_token. If context.Validated is called with ticket
        ///             information the response body is produced in the same way as the other standard grant types. If additional response parameters must be
        ///             included they may be added in the final TokenEndpoint call.
        ///             See also http://tools.ietf.org/html/rfc6749#section-4.5
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task GrantCustomExtension(OAuthGrantCustomExtensionContext context)
        {
            Console.WriteLine("In GrantCustomExtension");
            return base.GrantCustomExtension(context);
        }

        /// <summary>
        /// Called when a request to the Token endpoint arrives with a "grant_type" of "refresh_token". This occurs if your application has issued a "refresh_token"
        ///             along with the "access_token", and the client is attempting to use the "refresh_token" to acquire a new "access_token", and possibly a new "refresh_token".
        ///             To issue a refresh token the an Options.RefreshTokenProvider must be assigned to create the value which is returned. The claims and properties
        ///             associated with the refresh token are present in the context.Ticket. The application must call context.Validated to instruct the
        ///             Authorization Server middleware to issue an access token based on those claims and properties. The call to context.Validated may
        ///             be given a different AuthenticationTicket or ClaimsIdentity in order to control which information flows from the refresh token to
        ///             the access token. The default behavior when using the OAuthAuthorizationServerProvider is to flow information from the refresh token to
        ///             the access token unmodified.
        ///             See also http://tools.ietf.org/html/rfc6749#section-6
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            Console.WriteLine("In GrantRefreshToken");
            return base.GrantRefreshToken(context);
        }

        /// <summary>
        /// Called to determine if an incoming request is treated as an Authorize or Token
        ///             endpoint. If Options.AuthorizeEndpointPath or Options.TokenEndpointPath
        ///             are assigned values, then handling this event is optional and context.IsAuthorizeEndpoint and context.IsTokenEndpoint
        ///             will already be true if the request path matches.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task MatchEndpoint(OAuthMatchEndpointContext context)
        {
            Console.WriteLine("In MatchEndpoint");
            return base.MatchEndpoint(context);
        }

        /// <summary>
        /// Called at the final stage of a successful Token endpoint request. An application may implement this call in order to do any final
        ///             modification of the claims being used to issue access or refresh tokens. This call may also be used in order to add additional
        ///             response parameters to the Token endpoint's json response body.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            Console.WriteLine("In TokenEndpoint");
            return base.TokenEndpoint(context);
        }

        /// <summary>
        /// Called before the TokenEndpoint redirects its response to the caller.
        /// </summary>
        /// <param name="context"/>
        /// <returns/>
        public override Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context)
        {
            Console.WriteLine("In TokenEndpointResponse");
            return base.TokenEndpointResponse(context);
        }

        /// <summary>
        /// Called for each request to the Authorize endpoint to determine if the request is valid and should continue.
        ///             The default behavior when using the OAuthAuthorizationServerProvider is to assume well-formed requests, with
        ///             validated client redirect URI, should continue processing. An application may add any additional constraints.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task ValidateAuthorizeRequest(OAuthValidateAuthorizeRequestContext context)
        {
            Console.WriteLine("In ValidateAuthorizeRequest");
            return base.ValidateAuthorizeRequest(context);
        }

        /// <summary>
        /// Called to validate that the context.ClientId is a registered "client_id", and that the context.RedirectUri a "redirect_uri"
        ///             registered for that client. This only occurs when processing the Authorize endpoint. The application MUST implement this
        ///             call, and it MUST validate both of those factors before calling context.Validated. If the context.Validated method is called
        ///             with a given redirectUri parameter, then IsValidated will only become true if the incoming redirect URI matches the given redirect URI.
        ///             If context.Validated is not called the request will not proceed further.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            Console.WriteLine("In ValidateClientRedirectUri");
            return base.ValidateClientRedirectUri(context);
        }

        /// <summary>
        /// Called for each request to the Token endpoint to determine if the request is valid and should continue.
        ///             The default behavior when using the OAuthAuthorizationServerProvider is to assume well-formed requests, with
        ///             validated client credentials, should continue processing. An application may add any additional constraints.
        /// </summary>
        /// <param name="context">The context of the event carries information in and results out.</param>
        /// <returns>
        /// Task to enable asynchronous execution
        /// </returns>
        public override Task ValidateTokenRequest(OAuthValidateTokenRequestContext context)
        {
            Console.WriteLine("In ValidateTokenRequest");
            return base.ValidateTokenRequest(context);
        }
    }
}
