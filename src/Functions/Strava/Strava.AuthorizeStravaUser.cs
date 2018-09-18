using System.Linq;
using System.Threading.Tasks;
using BurnForMoney.Functions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Queue;

namespace BurnForMoney.Functions.Strava
{
    public static class AuthorizeStravaUser
    {
        private const int AuthorisationCodeLength = 40;
        private static TraceWriter _log;
        private const string StravaAuthorizationUrl = "https://www.strava.com/oauth/authorize";
        private const string AzureHostUrl = "https://functions.azure.com";
        private static ConfigurationRoot _configuration;

        [FunctionName("AuthorizeStravaUser")]
        public static async Task<IActionResult> RunAuthorizeStravaUser([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "strava/authorize")]
            HttpRequest req, TraceWriter log, [Queue(QueueNames.AuthorizationCodes)] CloudQueue authorizationCodesQueue,
            ExecutionContext context)
        {
            _log = log;
            _log.Info("AuthorizeStravaUser function processed a request.");
            LoadSettings(context);

            var referer = req.Headers["Referer"].FirstOrDefault();
            if (!_configuration.IsLocalEnvironment && !IsRequestRefererValid(referer))
            {
                log.Warning($"Request referer [{referer ?? "null"}] is not authorized.");
                return new UnauthorizedResult();
            }

            string code = req.Query["code"];
            if (string.IsNullOrWhiteSpace(code))
            {
                log.Warning("Function invoked with incorrect parameters. [code] is null or empty.");
                return new BadRequestObjectResult("Code is required.");
            }

            if (code.Length != AuthorisationCodeLength)
            {
                log.Warning($"The provided code is invalid. Authorisation code should be {AuthorisationCodeLength} chars long, but was {code.Length}.");
                return new BadRequestObjectResult("The provided code is invalid.");
            }

            await InsertCodeToAuthorizationQueueAsync(code, authorizationCodesQueue).ConfigureAwait(false);

            return new OkObjectResult("Ok");
        }

        private static void LoadSettings(ExecutionContext context)
        {
            if (_configuration != null)
            {
                return;
            }

            _configuration = new ApplicationConfiguration().GetSettings(context);
        }

        private static async Task InsertCodeToAuthorizationQueueAsync(string code, CloudQueue queue)
        {
            var message = new CloudQueueMessage(code);
            await queue.AddMessageAsync(message).ConfigureAwait(false);
            _log.Info($"Inserted authorization code to {QueueNames.AuthorizationCodes} queue.");
        }

        private static bool IsRequestRefererValid(string referer) => !string.IsNullOrEmpty(referer) && (referer.StartsWith(StravaAuthorizationUrl) || referer.StartsWith(AzureHostUrl));
    }
}