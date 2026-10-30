using System;
using System.Configuration;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using CodeLab.Models;

namespace CodeLab.Services
{
    public sealed class JDoodleExecutionService
    {
        private const string ExecuteUrl =
            "https://api.jdoodle.com/v1/execute";

        private const int MaximumCodeLength = 20000;

        private static readonly HttpClient Client = CreateClient();

        public async Task<CodeExecutionResult> ExecuteAsync(
            string language,
            string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                return Failure("Enter some code before running it.");
            }

            if (sourceCode.Length > MaximumCodeLength)
            {
                return Failure(
                    "Your code is too long. The limit is 20,000 characters.");
            }

            string normalisedLanguage =
                (language ?? string.Empty).Trim().ToLowerInvariant();

            if (normalisedLanguage != "python" &&
                normalisedLanguage != "python3")
            {
                return Failure(
                    "Code execution currently supports Python challenges only.");
            }

            string clientId =
                ConfigurationManager.AppSettings["JDoodle.ClientId"];

            string clientSecret =
                ConfigurationManager.AppSettings["JDoodle.ClientSecret"];

            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret) ||
                clientId == "YOUR_CLIENT_ID" ||
                clientSecret == "YOUR_CLIENT_SECRET")
            {
                return Failure(
                    "Code execution is not configured. " +
                    "Check the local JDoodle settings.");
            }

            // A version index identifies a runtime in JDoodle's list.
            // Index 0 is the first listed Python 3 runtime, not the latest.
            string versionIndex =
                ConfigurationManager.AppSettings["JDoodle.PythonVersionIndex"];

            if (string.IsNullOrWhiteSpace(versionIndex))
            {
                versionIndex = "0";
            }

            int parsedVersion;

            if (!int.TryParse(
                    versionIndex,
                    NumberStyles.None,
                    CultureInfo.InvariantCulture,
                    out parsedVersion) ||
                parsedVersion < 0)
            {
                return Failure(
                    "The configured Python runtime version is invalid.");
            }

            var serializer = new JavaScriptSerializer
            {
                MaxJsonLength = 1024 * 1024
            };

            var payload = new
            {
                clientId = clientId.Trim(),
                clientSecret = clientSecret.Trim(),
                script = sourceCode,
                language = "python3",
                versionIndex = parsedVersion.ToString(
                    CultureInfo.InvariantCulture),
                stdin = string.Empty
            };

            try
            {
                string json = serializer.Serialize(payload);

                using (var request = new HttpRequestMessage(
                    HttpMethod.Post,
                    ExecuteUrl))
                {
                    request.Content = new StringContent(
                        json,
                        Encoding.UTF8,
                        "application/json");

                    using (var response = await Client.SendAsync(request))
                    {
                        int httpStatus = (int)response.StatusCode;

                        if (!response.IsSuccessStatusCode)
                        {
                            return HttpFailure(httpStatus);
                        }

                        string responseJson =
                            await response.Content.ReadAsStringAsync();

                        JDoodleResponse result;

                        try
                        {
                            result = serializer
                                .Deserialize<JDoodleResponse>(responseJson);
                        }
                        catch (ArgumentException)
                        {
                            return Failure(
                                "The execution service returned an unreadable response.");
                        }
                        catch (InvalidOperationException)
                        {
                            return Failure(
                                "The execution service returned an unreadable response.");
                        }

                        if (result == null)
                        {
                            return Failure(
                                "The execution service returned an empty response.");
                        }

                        if (!string.IsNullOrWhiteSpace(result.error))
                        {
                            // Do not display raw provider errors or credentials.
                            return Failure(
                                "The execution service could not process the request. " +
                                "Check your API access and remaining credits.",
                                result.statusCode);
                        }

                        if (result.statusCode != 200)
                        {
                            return HttpFailure(result.statusCode ?? 0);
                        }

                        return new CodeExecutionResult
                        {
                            RequestSucceeded = true,
                            StatusCode = result.statusCode,
                            Output = result.output ?? string.Empty,
                            CpuTime = result.cpuTime ?? string.Empty,
                            Memory = result.memory ?? string.Empty,
                            ErrorMessage = string.Empty
                        };
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Do not automatically retry: the provider may have
                // already executed the code and consumed a credit.
                return Failure(
                    "The execution request timed out. " +
                    "It may have used a credit. Please try again later.");
            }
            catch (HttpRequestException)
            {
                return Failure(
                    "Unable to contact the execution service. " +
                    "Check your internet connection and try again.");
            }
        }

        private static HttpClient CreateClient()
        {
            return new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30),
                MaxResponseContentBufferSize = 1024 * 1024
            };
        }

        private static CodeExecutionResult HttpFailure(int statusCode)
        {
            string message;

            switch (statusCode)
            {
                case 401:
                case 403:
                    message =
                        "JDoodle rejected the request. " +
                        "Check your credentials and API access.";
                    break;

                case 402:
                case 429:
                    message =
                        "The execution service's usage or rate limit was reached. " +
                        "Check your remaining credits and try again later.";
                    break;

                case 400:
                    message =
                        "The execution service rejected the code request.";
                    break;

                default:
                    message =
                        "The execution service is temporarily unavailable. " +
                        "Please try again later.";
                    break;
            }

            return Failure(message, statusCode);
        }

        private static CodeExecutionResult Failure(
            string message,
            int? statusCode = null)
        {
            return new CodeExecutionResult
            {
                RequestSucceeded = false,
                ErrorMessage = message,
                StatusCode = statusCode
            };
        }

        public sealed class JDoodleResponse
        {
            public string output { get; set; }

            public int? statusCode { get; set; }

            public string memory { get; set; }

            public string cpuTime { get; set; }

            public string error { get; set; }
        }
    }
}