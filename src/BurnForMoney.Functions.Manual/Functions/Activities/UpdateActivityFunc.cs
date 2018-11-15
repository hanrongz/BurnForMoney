﻿using System;
using System.Threading.Tasks;
using BurnForMoney.Functions.Shared.Extensions;
using BurnForMoney.Functions.Shared.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace BurnForMoney.Functions.Manual.Functions.Activities
{
    public static class UpdateActivityFunc
    {
        [FunctionName(QueueNames.UpdateActivity)]
        public static async Task<IActionResult> Async([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "athlete/{athleteId:length(32)}/activities/{activityId:length(32)}")] HttpRequest req, ExecutionContext executionContext,
            string athleteId, string activityId,
            ILogger log,
            [Queue(AppQueueNames.UpdateActivityRequests)] CloudQueue outputQueue)
        {
            log.LogFunctionStart(QueueNames.UpdateActivity);

            var requestData = await req.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<UpdateActivityRequest>(requestData);
            try
            {
                ValidateRequest(model);
            }
            catch (Exception ex)
            {
                log.LogError(QueueNames.UpdateActivity, ex.Message);
                return new BadRequestObjectResult($"Validation failed. {ex.Message}.");
            }

            var pendingActivity = new PendingRawActivity
            {
                Id = activityId,
                ExternalId = model.ExternalId,
                ActivityType = model.ActivityCategory,
                StartDate = model.StartDate.Value,
                DistanceInMeters = model.DistanceInMeters,
                MovingTimeInMinutes = model.MovingTimeInMinutes,
                Source = "Manual"
            };

            var output = JsonConvert.SerializeObject(pendingActivity);
            await outputQueue.AddMessageAsync(new CloudQueueMessage(output));
            log.LogFunctionEnd(QueueNames.UpdateActivity);
            return new OkObjectResult(pendingActivity.Id);
        }

        private static void ValidateRequest(UpdateActivityRequest request)
        {
            if (request.StartDate == null)
            {
                throw new ArgumentNullException(nameof(request.StartDate));
            }
            if (string.IsNullOrWhiteSpace(request.ActivityCategory))
            {
                throw new ArgumentNullException(nameof(request.ActivityCategory));
            }
            if (request.MovingTimeInMinutes <= 0)
            {
                throw new ArgumentNullException(nameof(request.MovingTimeInMinutes));
            }
        }

        public class UpdateActivityRequest
        {
            public string ExternalId { get; set; }
            public DateTime? StartDate { get; set; }
            public string ActivityCategory { get; set; }
            public double DistanceInMeters { get; set; }
            public double MovingTimeInMinutes { get; set; }
        }
    }
}