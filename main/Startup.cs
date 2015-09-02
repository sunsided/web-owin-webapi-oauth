using System.Web.Http;
using JetBrains.Annotations;
using Owin;

namespace main
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
            // Configure Web API for self-host.
            HttpConfiguration config = new HttpConfiguration();
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });

            appBuilder.UseWebApi(config);
        }
    }
}
