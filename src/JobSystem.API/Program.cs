using JobSystem.Application.Interfaces;
using JobSystem.Application.Jobs;
using JobSystem.Infrastructure.Messaging;
using JobSystem.Infrastructure.Persistence;
using JobSystem.Infrastructure.Repositories;
using JobSystem.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;


namespace JobSystem.API;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting API");

            var builder = WebApplication.CreateBuilder(args);

            // Integrate Serilog
            builder.Host.UseSerilog();

            // Database - read connection string from configuration (.env via docker-compose env_file)
            var connectionString = builder.Configuration.GetConnectionString("Default")
                                   ?? builder.Configuration["ConnectionStrings:Default"]
                                   ?? throw new InvalidOperationException("Connection string 'Default' not configured.");

            builder.Services.AddDbContext<JobDbContext>(options =>
                options.UseNpgsql(connectionString));

            // Dependency Injection
            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddScoped<IJobQueue, RabbitMqJobQueue>();
            builder.Services.AddScoped<CreateJobHandler>();
            builder.Services.AddHostedService<JobWorker>();
            builder.Services.AddControllers();


            builder.Services.AddHealthChecks()
                .AddNpgSql(
                    builder.Configuration.GetConnectionString("Default"),
                    name: "postgres",
                    timeout: TimeSpan.FromSeconds(5))
                .AddRabbitMQ(
                     $"amqp://{builder.Configuration["RabbitMq:UserName"]}:" +
                    $"{builder.Configuration["RabbitMq:Password"]}@" +
                    $"{builder.Configuration["RabbitMq:HostName"]}",
                    name: "rabbitmq",
                    timeout: TimeSpan.FromSeconds(5));
             
                    

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<JobDbContext>();
                db.Database.Migrate();
            }
            
            // Basic middleware pipeline
            app.UseSerilogRequestLogging();

            app.UseRouting();
            app.MapControllers();
            app.MapHealthChecks("/health");

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "API terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}