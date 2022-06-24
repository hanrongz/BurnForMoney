# Idempotency Violation Tests for [BurnForMoney](https://github.com/makingwaves/BurnForMoney)

## Duplicate emails

### [B_SendNotificationWithLinkToTheReport](https://github.com/makingwaves/BurnForMoney/blob/master/src/BurnForMoney.Functions/Functions/Reports/ReportGeneratorFunc.cs#L131) + [Q_NotificationsGateway](https://github.com/makingwaves/BurnForMoney/blob/2223ee3f5efe340dec5909379e1e7da8bda4078e/src/BurnForMoney.Functions/Functions/Notifications/NotificationsGatewayFunc.cs#L23)

#### `B_SendNotificationWithLinkToTheReport` is non-idempotent.

If `B_SendNotificationWithLinkToTheReport` fails after a new notification is added to the queue, and then retries, messages will accumulate in `notificationsQueue`. `SendEmail` will find duplicate notifications in `NotificationsToSend` and send duplicate emails to the user.

In cases where `B_SendNotificationWithLinkToTheReport` keeps failing, the number of messages reaches the maximum dequeue count and messages are moved to poison queue, the user will receive five duplicate emails.

Log:

> Message has reached MaxDequeueCount of 5. Moving message to queue 'webjobs-blobtrigger-poison'

#### `Q_NotificationsGateway` is non-idempotent.

If `SendEmail` fails after a new message is added to the queue, and then retries, messages will accumulate in `NotificationsToSend`. In cases where `SendEmail` keeps failing, the number of messages reaches the maximum dequeue count and messages are moved to poison queue, the user will receive five duplicate emails.

Log:

>  Message has reached MaxDequeueCount of 5. Moving message to queue 'notifications-to-send-poison'.

#### Testing

1. Clone this repository and set up connections in `local.settings.json` as follows:

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

2. Create a resource container named `reports` in the storage account. 
3. To test `B_SendNotificationWithLinkToTheReport`, uncomment [ReportGeneratorFunc.cs#L143](https://github.com/hanrongz/BurnForMoney/blob/6fe597803c39df82ebedd1da6917f4ca1f4882a9/src/BurnForMoney.Functions/Functions/Reports/ReportGeneratorFunc.cs#L143); To test `Q_NotificationsGateway`, uncomment [NotificationsGatewayFunc.cs#L44](https://github.com/hanrongz/BurnForMoney/blob/bc22d1bf4dff8f5d4fa6528afdf6d4a498f9be0c/src/BurnForMoney.Functions/Functions/Notifications/NotificationsGatewayFunc.cs#L44).
4. Run the application.
5. Upload a blob to the container.
6. The reports receiver will receive five duplicate emails.

### References

[Poison messages](https://docs.microsoft.com/en-us/azure/azure-functions/functions-bindings-storage-queue-trigger?tabs=in-process%2Cextensionv5&pivots=programming-language-csharp)

