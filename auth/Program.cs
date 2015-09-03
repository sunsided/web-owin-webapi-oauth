using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Protocols.WSTrust;
using System.Net;
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

                PrintStatusMessage("OAuth: Attempting to retrieve a bearer token for sample audience ...", false);
                var requestTask = TestRequestToken(baseAddress, audienceId: "099153c2625149bc8ecb3e85e03f0022");
                requestTask.Wait();
                ExpectResult(requestTask.Result, HttpStatusCode.OK);

                PrintStatusMessage($"API: Attempting to register new audience '{newAudienceName}' ...");
                var registerTask = TestApi(baseAddress, newAudienceName);
                registerTask.Wait();
                ExpectResult(registerTask.Result.Item1, HttpStatusCode.OK);
                var audience = registerTask.Result.Item2;

                if (audience != null)
                {
                    PrintStatusMessage($"OAuth: Attempting to retrieve a bearer token for audience '{audience.Name}' ...");
                    requestTask = TestRequestToken(baseAddress, audienceId: audience.ClientId);
                    requestTask.Wait();
                    ExpectResult(requestTask.Result, HttpStatusCode.OK);
                }

                PrintStatusMessage("OAuth: Attempting to retrieve a bearer token for a non-existing audience ...");
                requestTask = TestRequestToken(baseAddress, audienceId: "something I just made up");
                requestTask.Wait();
                ExpectResult(requestTask.Result, HttpStatusCode.BadRequest);

                Console.ReadKey(true);
            }
        }

        /// <summary>
        /// Gibt eine Fehler- oder Erfolgsmeldung aus
        /// </summary>
        /// <param name="statusCode">The status code.</param>
        /// <param name="expectedCode">The expected code.</param>
        private static void ExpectResult(HttpStatusCode statusCode, HttpStatusCode expectedCode)
        {
            if (statusCode == expectedCode)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write($"OK - Erwartet: {expectedCode}, erhalten: {statusCode}");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($"Fehler - Erwartet: {expectedCode}, erhalten: {statusCode}");
            }
            Console.ResetColor();
            Console.WriteLine();
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
        private static async Task<HttpStatusCode> TestRequestToken([NotNull] string baseAddress, [NotNull] string audienceId)
        {
            // Create HttpCient and make a request to api/values
            using (var client = new HttpClient(new LoggingHandler(new HttpClientHandler())))
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

                return response.StatusCode;
            }
        }

        /// <summary>
        /// Tests the API.
        /// </summary>
        /// <param name="baseAddress">The base address.</param>
        /// <param name="friendlyName">The friendly name of the <see cref="Audience"/>.</param>
        /// <returns>The task that represents this operation.</returns>
        [NotNull]
        private static async Task<Tuple<HttpStatusCode, Audience>> TestApi([NotNull] string baseAddress, [NotNull] string friendlyName)
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

                return Tuple.Create(
                    response.StatusCode,
                    response.IsSuccessStatusCode ? JsonConvert.DeserializeObject<Audience>(content) : null);
            }
        }
    }
}
