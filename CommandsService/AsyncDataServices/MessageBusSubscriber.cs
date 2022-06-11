using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommandsService.EventProcessing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace CommandsService.AsyncDataServices
{
    public class MessageBusSubscriber : BackgroundService
    {
        private readonly IConfiguration configuration;
        private readonly IEventProcessor eventProcessor;
        private IConnection connection;
        private IModel channel;
        private string queueName;

        public MessageBusSubscriber(
            IConfiguration configuration,
            IEventProcessor eventProcessor)
        {
            this.eventProcessor = eventProcessor;
            this.configuration = configuration;

            InitializeRabbitMQ();
        }

        private void InitializeRabbitMQ()
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQHost"],
                Port = int.Parse(configuration["RabbitMQPort"])
            };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);
            queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName, exchange: "trigger", routingKey: "");

            System.Console.WriteLine("--> Listening on the message bus...");

            connection.ConnectionShutdown += RabbitMq_ConnectionShutdown;
        }

        private void RabbitMq_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            System.Console.WriteLine("Connection Shutdwon");
        }

        public override void Dispose()
        {
            if (channel.IsOpen)
            {
                channel.Close();
                connection.Close();
            }
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (ModuleHandle, ea) => 
            {
                System.Console.WriteLine("--> Event received!");
                var body = ea.Body;
                var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

                eventProcessor.ProcessEvent(notificationMessage);
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

            return Task.CompletedTask;
        }
    }
}