using System;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration configuration;
        private readonly IConnection connection;
        private readonly IModel channel;

        public MessageBusClient(IConfiguration configuration)
        {
            this.configuration = configuration;
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQHost"],
                Port = int.Parse(configuration["RabbitMQPort"])
            };

            try
            {
                connection = factory.CreateConnection();
                channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                System.Console.WriteLine("--> Connected to MessageBus");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine("--> RabbitMQ Connection shutdown");
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);
            if (connection.IsOpen)
            {
                System.Console.WriteLine("--> RabbitMQ Connection Open, sending message...");
                sendMessage(message);
            }
            else
            {
                System.Console.WriteLine("--> RabbitMQ Connection is closed, not sending...");
            }
        }

        private void sendMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "trigger", 
                routingKey: "", 
                basicProperties: null, 
                body: body);

            System.Console.WriteLine($"--> We have send {message}");
        }

        public void Dispose()
        {
            System.Console.WriteLine("--> Message Bus disposed");
            if(channel.IsOpen)
            {
                channel.Close();
                connection.Close();
            }
        }
    }
}