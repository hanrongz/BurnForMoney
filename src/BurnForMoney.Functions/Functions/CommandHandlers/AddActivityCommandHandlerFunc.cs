﻿using System.Threading.Tasks;
using BurnForMoney.Functions.Shared.Extensions;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using BurnForMoney.Functions.CommandHandlers;
using BurnForMoney.Functions.Repositories;
using BurnForMoney.Functions.Commands;
using BurnForMoney.Functions.Infrastructure.Queues;

namespace BurnForMoney.Functions.Functions.CommandHandlers
{
    public static class AddActivityCommandHandlerFunc
    {
        [FunctionName(FunctionsNames.Q_AddActivity)]
        public static async Task Q_AddActivity(ILogger log, ExecutionContext executionContext,
            [QueueTrigger(AppQueueNames.AddActivityRequests)] AddActivityCommand message)
        {
            var repository = AthleteRepositoryFactory.Create();
            var commandHandler = new AddActivityCommandHandler(repository);
            await commandHandler.HandleAsync(message);
        }
    }
}