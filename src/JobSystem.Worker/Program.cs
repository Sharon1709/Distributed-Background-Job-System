using JobSystem.Application.Interfaces;
using JobSystem.Infrastructure.Persistence;
using JobSystem.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace JobSystem.Worker;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting Worker Service");

            var builder = Host.CreateApplicationBuilder(args);

            // Use Serilog properly
            builder.Services.AddSerilog();

            builder.Services.AddDbContext<JobDbContext>(options =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("Default")));

            builder.Services.AddScoped<IJobRepository, JobRepository>();
            builder.Services.AddHostedService<JobWorker>();

            var host = builder.Build();
            host.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Worker terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}