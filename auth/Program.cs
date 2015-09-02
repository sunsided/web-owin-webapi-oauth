using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;

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
                var audienceModel = JsonConvert.SerializeObject(new
                                                                    {
                                                                        Name = "Something 9000"
                                                                    });

                var request = new StringContent(audienceModel, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseAddress + "api/audience", request);
                Console.WriteLine(response);

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }
    }
}
