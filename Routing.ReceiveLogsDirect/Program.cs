﻿using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.ExchangeDeclare("direct_logs", ExchangeType.Direct);

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage: {0} [info] [warning] [error]", Environment.GetCommandLineArgs()[0]);
    Console.WriteLine(" Press [enter] to exit.");
    Console.ReadLine();
    Environment.ExitCode = 1;
    return;
}

var queueName = channel.QueueDeclare().QueueName;
foreach (var severity in args)
{
    channel.QueueBind(
        queue: queueName, 
        exchange: "direct_logs",
        routingKey: severity);
}

Console.WriteLine(" [*] Waiting for messages.");

var consumer = new EventingBasicConsumer(channel);
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    var message = Encoding.UTF8.GetString(body);
    var routingKey = ea.RoutingKey;
    Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
};
Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();