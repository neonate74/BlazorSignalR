using BlazorApp.Server.Hubs;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using System.Security.Principal;

namespace Server.Services;

public class TimedHostedService : IHostedService, IDisposable
{
    private int executionCount = 0;
    private readonly ILogger<TimedHostedService> _logger;
    private IHubContext<ChatHub> _hubConnection;
    private Timer _timer;

    public TimedHostedService(ILogger<TimedHostedService> logger, IHubContext<ChatHub> chatHubContext)
    {
        _logger = logger;

        _hubConnection = chatHubContext;
    }

    public Task StartAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service running.");

        _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    private async void DoWork(object state)
    {
        var count = Interlocked.Increment(ref executionCount);

        await _hubConnection.Clients.All.SendAsync("ReceiveMessage", "Srv", "", "Test");

        _logger.LogInformation("Timed Hosted Service is working. Count: {Count}", count);
    }

    public Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Timed Hosted Service is stopping.");

        _timer?.Change(Timeout.Infinite, 0);

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}