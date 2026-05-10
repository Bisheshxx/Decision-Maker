using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using DecisionMaker.Data;

namespace DecisionMaker.Services;



public class SupabaseKeepAliveService : BackgroundService

{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SupabaseKeepAliveService> _logger;

    public SupabaseKeepAliveService(IServiceScopeFactory scopeFactory, ILogger<SupabaseKeepAliveService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                _logger.LogError("The Supabase logger is running....");
                await db.Database.ExecuteSqlRawAsync("SELECT 1;", stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError("Keep-alive failed: {Message}", ex.Message);
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}