using System;
using System.Web.Http;
using auth.Formats;
using auth.Providers;
using JetBrains.Annotations;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;
using Owin;
using Owin.Security.AesDataProtectorProvider;

namespace auth
{
    /// <summary>
    /// OWIN / Web API 2 Startup
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Configures Web API.
        /// </summary>
        /// <remarks>
        /// The Startup class is specified as a type parameter in the WebApp.Start method.
        /// </remarks>
        /// <param name="appBuilder">
        /// The application builder.
        /// </param>
        public void Configuration([NotNull] IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();

            // Die Vorgabe für Data Protection ist, den Data Protection API-Provider zu verwenden.
            // Da es sich dabei um eine Windows-API handelt, ist diese nicht unter Mono verfügbar.
            // Der folgende Aufruf setzt den Data Protector-Provider auf AES.
            // https://github.com/i4004/Owin.Security.AesDataProtectorProvider
            appBuilder.UseAesDataProtectorProvider();

            // Web API routes
            config.MapHttpAttributeRoutes();

            ConfigureOAuth(appBuilder);

            appBuilder.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

            appBuilder.UseWebApi(config);

        }

        /// <summary>
        /// Configures OAuth.
        /// </summary>
        /// <param name="appBuilder">
        /// The application builder.
        /// </param>
        public void ConfigureOAuth(IAppBuilder appBuilder)
        {

            var serverOptions = new OAuthAuthorizationServerOptions
            {
                // For Dev enviroment only (on production should be AllowInsecureHttp = false)
#if DEBUG
                AllowInsecureHttp = true,
#else
                AllowInsecureHttp = false,
#endif
                TokenEndpointPath = new PathString("/oauth2/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(30),
                Provider = new CustomOAuthProvider(),
                AccessTokenFormat = new CustomJwtFormat("http://jwtauthzsrv.azurewebsites.net") // TODO: Die ID des Issuers muss konfigurierbar sein
            };

            // OAuth 2.0 Bearer Access Token Generation
            appBuilder.UseOAuthAuthorizationServer(serverOptions);
        }
    }
}
