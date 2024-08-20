using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Azure;

namespace ServiceBus.Api.Controllers;

public class TopicController : ControllerBase
{
    private readonly string _topicName;
    private readonly ServiceBusClient _serviceBusClient;

    public TopicController (IConfiguration configuration, IAzureClientFactory<ServiceBusClient> azureClientFactory)
    {
        _serviceBusClient = azureClientFactory.CreateClient("ServiceBusClient");
        _topicName = configuration["AzureServiceBus:TopicName"] ?? throw new InvalidOperationException("ServiceBus topic name is missing");
    }
    
    [HttpPost("Publish")]
    public async Task<IActionResult> Publish ([FromBody] List<string> contexts)
    {
        await using var sender = _serviceBusClient.CreateSender(_topicName);
        
        using var messageBatch = await sender.CreateMessageBatchAsync();
        foreach (var text in contexts)
        {
            var message = new ServiceBusMessage(text);
            if (messageBatch.TryAddMessage(message) is false)
            {
                throw new InvalidOperationException("The message is too large to fit in the batch.");
            }
        }

        await sender.SendMessagesAsync(messageBatch);

        return Ok();
    }
    
    [HttpGet("Dequeue")]
    public async Task<IActionResult> Dequeue ([FromQuery] string subscriptionName)
    {
        await using var receiver = _serviceBusClient.CreateReceiver(_topicName, subscriptionName);
        
        var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
        
        if (message is null)
        {
            return NoContent();
        }
        
        return Ok(Encoding.UTF8.GetString(message.Body));
    }
}