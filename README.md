# Idempotency Violation Tests for [BurnForMoney](https://github.com/makingwaves/BurnForMoney)

## Duplicate emails

### [B_SendNotificationWithLinkToTheReport](https://github.com/makingwaves/BurnForMoney/blob/master/src/BurnForMoney.Functions/Functions/Reports/ReportGeneratorFunc.cs#L131) + [SendEmail](https://github.com/makingwaves/BurnForMoney/blob/2223ee3f5efe340dec5909379e1e7da8bda4078e/src/BurnForMoney.Functions/Functions/Notifications/NotificationsGatewayFunc.cs#L23)

`B_SendNotificationWithLinkToTheReport` is non-idempotent.

If `B_SendNotificationWithLinkToTheReport` fails after a newly generated notification is added to the queue, and then retries, `SendEmail` will find duplicate notifications in queue and send duplicate emails to the user.

In cases where `B_SendNotificationWithLinkToTheReport` keeps failing, it retries up to five times and the user will receive up to five duplicate emails. To reproduce test results, clone this repository, uncomment [ReportGeneratorFunc.cs#L143](https://github.com/hanrongz/BurnForMoney/blob/2223ee3f5efe340dec5909379e1e7da8bda4078e/src/BurnForMoney.Functions/Functions/Reports/ReportGeneratorFunc.cs#L143) and set up connections in `local.settings.json` as follows:

```
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "AzureWebJobsStorage": "%AzureWebJobsStorageConnectionString%", // Create a storage account in azure portal
    "AzureWebJobsDashboard": "%AzureWebJobsStorageConnectionString%",
    "StravaQueuesStorage": "%AzureWebJobsStorageConnectionString%",
    "StravaAppHostName": "http://localhost:7072",
    "KeyVaultName": "%KeyVaultName%", // Create a key vault in azure portal
    "ConnectionStrings:Sql": "%AzureWebJobsStorageConnectionString%",
    "SendGrid:ApiKey": "%SendGridApiKey%" // Create a send grid in azure portal, authorize the email you want to use in "Email" section
  },
  "Email": {
    "SenderEmail": "%Email%",
    "ReportsReceiver": "%Email%",
    "DefaultRecipient": "%Email%"
  },
  "EventGrid": {
    "TopicEndpoint": "%EventGridURL%" // Create an event grid in azure portal
  },
  "Host": {
    "LocalHttpPort": 7071
  }
}
```

Create a resource container named `reports` in the storage account. Run the application. Upload a blob to the container. The reports receiver will receive five duplicate emails.

### References

[Poison messages](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp)

