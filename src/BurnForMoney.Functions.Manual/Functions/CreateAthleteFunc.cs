﻿using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;

namespace BurnForMoney.Functions.Manual.Functions
{
    public static class CreateAthleteFunc
    {
        [FunctionName("CreateAthlete")]
        public static IActionResult CreateAthlete([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "athletes")] HttpRequest req, ExecutionContext executionContext)
        {
            throw new NotImplementedException();
        }
    }
}