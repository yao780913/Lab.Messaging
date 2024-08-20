using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ServiceBus.FunctionApp;

public class ServiceBusQueueTrigger1
{
    private readonly ILogger<ServiceBusQueueTrigger1> _logger;
    private readonly IConfiguration _configuration;

    public ServiceBusQueueTrigger1 (ILogger<ServiceBusQueueTrigger1> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function(nameof(ServiceBusQueueTrigger1))]
    public void Run ([ServiceBusTrigger("%AzureServiceBus:QueueName%", Connection = "AzureServiceBus:ConnectionString")] ServiceBusReceivedMessage message)
    {
        _logger.LogInformation("Message ID: {Id}", message.MessageId);
        _logger.LogInformation("Message Body: {Body}", message.Body);
        _logger.LogInformation("Message Content-Type: {ContentType}", message.ContentType);
        
    }
}