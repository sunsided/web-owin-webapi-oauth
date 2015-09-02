using System;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Owin.Hosting;

namespace main
{
    /// <summary>
    /// The main entry point
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        internal static void Main(string[] args)
        {
            StartOptions options = new StartOptions();

            const int port = 9000;

            // NOTE: Bindungen müssen unter Windows explizit registriert werden. Symptomatisch für
            // eine fehlende Registrierung ist das Auftreten einer TargetInvocationException.
            // In diesem Fall muss folgender Befehl in einer Admin-Shell ausgeführt werden:
            // netsh http add urlacl url=http://+:9000/ user=machine\username
            options.Urls.Add($"http://+:{port}/");

            // Start the OWIN host
            using (WebApp.Start<Startup>(options))
            {
                var baseAddress = $"http://localhost:{port}/";
                var task = TestApi(baseAddress);
                task.Wait();
            }

            Console.ReadKey(true);
        }

        /// <summary>
        /// Tests the API.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <returns>The task that represents this operation.</returns>
        private static async Task TestApi([NotNull] string baseAddress)
        {
            // Create HttpCient and make a request to api/values
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(baseAddress + "api/values");
                Console.WriteLine(response);

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }
    }
}
