using System.Text;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;

namespace ServiceBus.Api.Controllers;

[ApiController]
public class QueueController : ControllerBase
{
    private readonly string? _connectionString;
    private readonly string? _queueName;

    public QueueController (IConfiguration configuration)
    {
        _connectionString = configuration["AzureServiceBus:ConnectionString"];
        _queueName = configuration["AzureServiceBus:QueueName"];
        
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException("ServiceBus connection string is missing");
        }
        
        if (string.IsNullOrWhiteSpace(_queueName))
        {
            throw new InvalidOperationException("ServiceBus queue name is missing");
        }
    }
    [HttpPost("Enqueue")]
    public async Task<IActionResult> Enqueue ([FromBody] List<string> contexts)
    {
        await using var client = new ServiceBusClient(_connectionString);
        await using var sender = client.CreateSender(_queueName);
        
        using var messageBatch = await sender.CreateMessageBatchAsync();
        foreach (var text in contexts)
        {
            var message = new ServiceBusMessage(text);
            if (messageBatch.TryAddMessage(message) is false)
            {
                throw new InvalidOperationException($"The message is too large to fit in the batch.");
            }
        }

        await sender.SendMessagesAsync(messageBatch);

        return Ok();
    }

    [HttpPost("Receive")]
    public async Task<IActionResult> Receive ()
    {
        await using var client = new ServiceBusClient(_connectionString);
        await using var receiver = client.CreateReceiver(_queueName);

        var message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(3));
        if (message is null)
        {
            return NoContent();
        }
        
        var body = message.Body.ToString();
        await receiver.CompleteMessageAsync(message);
        return Ok(body);
    }
    
    [HttpPost("ReceiveBatch")]
    public async Task<IActionResult> ReceiveBatch ([FromQuery]int len = 10)
    {
        await using var client = new ServiceBusClient(_connectionString);
        await using var receiver = client.CreateReceiver(_queueName);

        var messages = await receiver.ReceiveMessagesAsync(len, TimeSpan.FromSeconds(3));
        if (messages.Count == 0)
        {
            return NoContent();
        }

        var body = new StringBuilder();
        foreach (var message in messages)
        {
            body.AppendLine(message.Body.ToString());
            await receiver.CompleteMessageAsync(message);
        }

        return Ok(body.ToString());
    }
}