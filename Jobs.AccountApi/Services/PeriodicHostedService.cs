using Jobs.Core.Contracts.Providers;

namespace Jobs.AccountApi.Services;

public class PeriodicHostedService(ILogger<PeriodicHostedService> logger, IServiceScopeFactory factory) : BackgroundService
{
    private readonly TimeSpan _period = TimeSpan.FromSeconds(60);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(_period);
        while (
            !stoppingToken.IsCancellationRequested &&
            await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await using AsyncServiceScope asyncScope = factory.CreateAsyncScope();
                var repository = asyncScope.ServiceProvider.GetRequiredService<IApiKeyStorageServiceProvider>();
                var count = await repository.DeleteExpiredKeysAsync();
                logger.LogInformation(
                    $"{nameof(PeriodicHostedService)} DeleteExpiredKeysAsync - count: {count}");
            }
            catch (Exception ex)
            {
                logger.LogInformation(
                    $"Failed to execute PeriodicHostedService with exception message {ex.Message}. Good luck next round!");
            }
        }
    }
}