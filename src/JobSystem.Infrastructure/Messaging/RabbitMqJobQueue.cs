using JobSystem.Application.Interfaces;
using JobSystem.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;


namespace JobSystem.Infrastructure.Messaging
{
    public class RabbitMqJobQueue : IJobQueue
    {
        private readonly ConnectionFactory _factory;
        private const string QueueName = "jobs-queue";

        public RabbitMqJobQueue(IConfiguration configuration)
        {
            var host = configuration["RabbitMq:HostName"] ?? "rabbitmq";
            var user = configuration["RabbitMq:UserName"] ?? "guest";
            var pass = configuration["RabbitMq:Password"] ?? "guest";

            _factory = new ConnectionFactory()
            {
                HostName = host,
                UserName = user,
                Password = pass
            };
        }

        public Task PublishAsync(Guid jobId, CancellationToken cancellationToken)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(jobId.ToString());

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(
                exchange: "",
                routingKey: QueueName,
                basicProperties: properties,
                body: body);

            return Task.CompletedTask;
        }
    }
}