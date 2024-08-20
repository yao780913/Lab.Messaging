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
        await using var processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 1
        });
        
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;
        
        await processor.StartProcessingAsync();
        await processor.StopProcessingAsync();
        
        return Ok();
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

    private static Task ErrorHandler (ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }

    private static async Task MessageHandler (ProcessMessageEventArgs args)
    {
        var message = args.Message.Body.ToString();
        
        Console.WriteLine($"Received: {message}");
        
        await args.CompleteMessageAsync(args.Message);
    }
}