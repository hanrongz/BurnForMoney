﻿using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using BurnForMoney.Functions.Strava.Model;
using Dapper;
using Microsoft.Extensions.Logging;

namespace BurnForMoney.Functions.Strava.Repository
{
    public class ActivityRepository : IRepository
    {
        private readonly string _connectionString;
        private readonly ILogger _log;

        public ActivityRepository(string connectionString, ILogger log)
        {
            _connectionString = connectionString;
            _log = log;
        }

        public async Task BootstrapAsync()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync("CREATE TABLE dbo.[Strava.Activities] ([AthleteId][int] NOT NULL, [ActivityId][int] NOT NULL, [ActivityTime][datetime2], [ActivityType][nvarchar](50), [Distance][int], [MovingTime][int], FOREIGN KEY (AthleteId) REFERENCES dbo.[Strava.Athletes](AthleteId))")
                    .ConfigureAwait(false);

                await conn.ExecuteAsync(StoredProcedures.StoredProcedures.Strava_Activity_Insert)
                    .ConfigureAwait(false);

                _log.LogInformation("dbo.[Strava.Activities] table created.");
            }
        }

        public async Task InsertAsync(Activity activity)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.ExecuteAsync(nameof(StoredProcedures.StoredProcedures.Strava_Activity_Insert),
                        new
                        {
                            AthleteId = activity.Athlete.Id,
                            ActivityId = activity.Id,
                            ActivityTime = activity.StartDate,
                            ActivityType = activity.Type,
                            Distance = activity.Distance,
                            MovingTime = activity.MovingTime
                        }, commandType: CommandType.StoredProcedure)
                    .ConfigureAwait(false);

                _log.LogInformation($"Activity with id: {activity.Id} has been added.");
            }
        }
    }
}