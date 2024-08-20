using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ServiceBus.FunctionApp;

public class ServiceBusTopicTrigger1
{
    private readonly ILogger<ServiceBusTopicTrigger1> _logger;

    public ServiceBusTopicTrigger1 (ILogger<ServiceBusTopicTrigger1> logger)
    {
        _logger = logger;
    }

    [Function(nameof(ServiceBusTopicTrigger1))]
    public void Run ([ServiceBusTrigger("%AzureServiceBus:TopicName%", "Sub1", Connection = "AzureServiceBus:ConnectionString")] ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("Message ID: {id}", message.MessageId);
        _logger.LogInformation("Message Body: {body}", message.Body);
        _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);
        
    }
}