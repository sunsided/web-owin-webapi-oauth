using System;
using JetBrains.Annotations;
using Microsoft.Owin.Hosting;

namespace auth
{
    /// <summary>
    /// Class Program.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        internal static void Main([NotNull] string[] args)
        {
            var options = new StartOptions();
            const int port = 9001;

            // NOTE: Bindungen müssen unter Windows explizit registriert werden. Symptomatisch für
            // eine fehlende Registrierung ist das Auftreten einer TargetInvocationException.
            // In diesem Fall muss folgender Befehl in einer Admin-Shell ausgeführt werden:
            // netsh http add urlacl url=http://+:9001/ user=machine\username
            options.Urls.Add($"http://+:{port}/");

            // Start the OWIN host
            using (WebApp.Start<Startup>(options))
            {
                Console.ReadKey(true);
            }
        }
    }
}
