## RabbitMQ tutorial
### Introduction
- Prerequisites  
  `docker run -it --rm --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3.13-management`
### Tutorial - "Hello World!"
### Tutorial - Work Queues
#### Round-robin dispatching
- Run 2 workers
  ```shell
  # shell 1
  cd WorkQueues/Worker
  dotnet run
  # => Press [enter] to exit
  ```
  ```shell
  # shell 2
  cd WorkQueues/Worker
  dotnet run
  # => Press [enter] to exit
  ```
- Run NewTask
  ```shell
  # shell 3
  cd WorkQueues/NewTask
  dotnet run "First message."
  dotnet run "Second message.."
  dotnet run "Third message..."
  dotnet run "Fourth message...."
  dotnet run "Fifth message....."
  ```
- Result
  ```shell
  # shell 1
  # => Press [enter] to exit.
  # => [x] Received First message.
  # => [x] Done
  # => [x] Received Third message...
  # => [x] Done
  # => [x] Received Fifth message.....
  # => [x] Done
  ```
  ```shell
  # shell 2
  # => Press [enter] to exit.
  # => [x] Received Second message..
  # => [x] Done
  # => [x] Received Fourth message....
  # => [x] Done
  ```
  
## Azure Service Bus
https://igouist.github.io/post/2022/08/azure-service-bus/

### Queue
#### ServiceBus.Api
##### Install NuGet packages:
- `Microsoft.Extensions.Azure`
- `Microsoft.Azure.ServiceBus`
##### Set user secrets:
```json
{
"AzureServiceBus:ConnectionString": "...",
"AzureServiceBus:QueueName": "...",
"AzureServiceBus:TopicName": "..."
}
```
##### Features
1. initialize `ServiceBusClient`
    1. `await using var client = new ServiceBusClient(_connectionString)`;
    2. using `AzureClientFactoryBuilder` to create `ServiceBusClient` in `Program.cs`, then inject `IAzureClientFactory<ServiceBusClient>` to create instance of `ServiceBusClient`
2. send a message to the queue
    1. using `ServiceBusSender` to send a message to the queue (`POST /queue/enqueue`)
3. receive a message from the queue
    1. using `ServiceBusReceiver` to receive a message from the queue (`GET /queue/ReceiveBatch`)
    2. using `ServiceBusProcessor` to receive messages from the queue (`GET /queue/Receive`)
    3. using `ServiceBusTrigger`(Azure Function App) to receive messages from the queue~~~~

> If you do not call receiver.CompleteMessageAsync(message) in Azure Service Bus, the message will not be marked as completed. This means the message will remain in the queue and will be available for redelivery. Essentially, the message will be considered as not processed successfully, and it will be retried according to the retry policy configured for the queue. This can lead to duplicate processing of the same message.

#### ServiceBus.FunctionApp
- `ServiceBusQueueTrigger` and `ServiceBusTopicTrigger`
- To define `queueName` and `connectionString`, use `local.settings.json`, `queueName` must surround by `%` symbol
```csharp
[Function(nameof(ServiceBusQueueTrigger1))]
public void Run ([ServiceBusTrigger("%QueueName%", Connection = "ConnectionString")] ServiceBusReceivedMessage message)
{
    // to do something ...    
}

// local.settings.json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet",
        "ConnectionString": "...",
        "QueueName": "..."
    }
}
```