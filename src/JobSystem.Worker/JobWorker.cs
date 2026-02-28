using JobSystem.Application.Interfaces;
using JobSystem.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;
using System.Text;

namespace JobSystem.Worker;

public class JobWorker : BackgroundService
{
    private const string QueueName = "jobs-queue";
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public JobWorker(IServiceScopeFactory scopeFactory, IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var host = _configuration["RabbitMq:HostName"];
        var user = _configuration["RabbitMq:UserName"];
        var pass = _configuration["RabbitMq:Password"];

        var factory = new ConnectionFactory()
        {
            HostName = host,
            UserName = user,
            Password = pass,
            Port = int.Parse(_configuration["RabbitMq:Port"] ?? "5672"),
            Ssl = new SslOption
            {
                Enabled = bool.Parse(_configuration["RabbitMq:Ssl:Enabled"] ?? "false")
            }
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                break;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "RabbitMQ not ready. Retrying in 5 seconds...");
                await Task.Delay(5000, stoppingToken);
            }
        }

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.BasicQos(0, 1, false);

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var jobIdString = Encoding.UTF8.GetString(body);

            if (!Guid.TryParse(jobIdString, out var jobId))
            {
                _channel.BasicAck(ea.DeliveryTag, false);
                return;
            }

            try
            {
                await ProcessJobAsync(jobId);
                _channel.BasicAck(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Processing failed for job {JobId}", jobId);
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: false,
            consumer: consumer);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _connection?.Close();
        return base.StopAsync(cancellationToken);
    }

    private async Task ProcessJobAsync(Guid jobId)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

        var job = await repository.GetByIdAsync(jobId, CancellationToken.None);

        if (job.Status == JobStatus.Completed)
        {
            Log.Information("Job {JobId} already completed. Skipping.", jobId);
            return;
        }

        if (job == null)
            return;

        job.MarkProcessing();
        await repository.UpdateAsync(job, CancellationToken.None);

        try
        {
            // Simulate random failure
            if (Random.Shared.Next(1, 4) == 1)
                throw new Exception("Random simulated failure");

            await Task.Delay(2000);

            job.MarkCompleted();
            await repository.UpdateAsync(job, CancellationToken.None);

            Log.Information("Processed job {JobId}", jobId);
        }
        catch (Exception ex)
        {
            job.MarkFailed(ex.Message);
            await repository.UpdateAsync(job, CancellationToken.None);

            if (job.CanRetry())
            {
                Console.WriteLine($"Retrying Job {jobId} - Attempt {job.RetryCount}");

                // Simple backoff delay
                await Task.Delay(job.RetryCount * 2000);

                throw; // Important → triggers Nack → requeue
            }
            else
            {
                //Console.WriteLine($"Job {jobId} moved to Dead Letter Queue");
                Log.Information("Job {jobId} moved to Dead Letter Queue", jobId);

                await MoveToDeadLetterQueue(jobId);

                // ACK so it doesn't retry again
            }
        }
    }
    private Task MoveToDeadLetterQueue(Guid jobId)
    {
        const string dlq = "jobs-dead-letter";

        _channel!.QueueDeclare(
            queue: dlq,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var body = Encoding.UTF8.GetBytes(jobId.ToString());

        _channel.BasicPublish("", dlq, null, body);

        return Task.CompletedTask;
    }
}