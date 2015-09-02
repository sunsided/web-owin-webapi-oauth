using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using auth.Entities;
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
            Trace.Listeners.Add(new ConsoleTraceListener());

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
                var newAudienceName = "Something 9000";

                PrintStatusMessage("Attempting to retrieve a bearer token for sample audience ...", false);
                var requestTask = TestRequestToken(baseAddress, audienceId: "099153c2625149bc8ecb3e85e03f0022");
                requestTask.Wait();

                PrintStatusMessage($"Attempting to register new audience '{newAudienceName}' ...");
                var registerTask = TestApi(baseAddress, newAudienceName);
                registerTask.Wait();
                var audience = registerTask.Result;

                if (audience != null)
                {
                    PrintStatusMessage($"Attempting to retrieve a bearer token for audience '{audience.Name}' ...");
                    requestTask = TestRequestToken(baseAddress, audienceId: audience.ClientId);
                    requestTask.Wait();
                }

                PrintStatusMessage("Attempting to retrieve a bearer token for a non-existing audience ...");
                requestTask = TestRequestToken(baseAddress, audienceId: "something I just made up");
                requestTask.Wait();

                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Prints a status message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="clearLine">if set to <c>true</c> [clear line].</param>
        private static void PrintStatusMessage([NotNull] string message, bool clearLine = true)
        {
            if (clearLine) Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        /// <summary>
        /// Versucht, ein Token zu beziehen.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="audienceId">The audience identifier.</param>
        /// <returns>The task that represents this operation.</returns>
        [NotNull]
        private static async Task TestRequestToken([NotNull] string baseAddress, [NotNull] string audienceId)
        {
            // Create HttpCient and make a request to api/values
            using (var client = new HttpClient())
            {
                var request = new FormUrlEncodedContent(new []
                                                            {
                                                                new KeyValuePair<string, string>("username", "chucknorris"),
                                                                new KeyValuePair<string, string>("password", "geheim"),
                                                                new KeyValuePair<string, string>("grant_type", "password"),
                                                                new KeyValuePair<string, string>("client_id", audienceId),
                                                            });

                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await client.PostAsync(baseAddress + "oauth2/token", request);
                Console.WriteLine(response);

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);
            }
        }

        /// <summary>
        /// Tests the API.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="friendlyName">The friendly name of the <see cref="Audience"/>.</param>
        /// <returns>The task that represents this operation.</returns>
        [NotNull]
        private static async Task<Audience> TestApi([NotNull] string baseAddress, [NotNull] string friendlyName)
        {
            // Create HttpCient and make a request to api/values
            using (var client = new HttpClient())
            {
                var audienceModel = JsonConvert.SerializeObject(new
                                                                    {
                                                                        Name = friendlyName
                                                                    });

                var request = new StringContent(audienceModel, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(baseAddress + "api/audience", request);
                Console.WriteLine(response);

                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<Audience>(content);
                }
            }

            return null;
        }
    }
}
