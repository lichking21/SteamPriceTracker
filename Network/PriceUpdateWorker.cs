using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using DataBaseOperator;
using DataBaseOperator.Entities;

namespace Network;

public class PriceUpdateWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly Price _price;
    private readonly ILogger<PriceUpdateWorker> _logger;

    private readonly TimeSpan _requestSendDelay = TimeSpan.FromSeconds(60);
    private readonly TimeSpan _pricesUpdateDelay = TimeSpan.FromHours(2);

    public PriceUpdateWorker(IServiceScopeFactory scopeFactory, Price price, ILogger<PriceUpdateWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _price = price;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken _stopToken)
    {
        _logger.LogInformation("(LOG) >>> Background price updating started");

        while (!_stopToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<TrackedGamesDB>();

                    var listIds = await db.GetIDs();
                    int count = listIds.Count;
                    _logger.LogInformation($"(LOG) >>> Found {count} items in tracking_games.");

                    if (count == 0)
                    {
                        await Task.Delay(TimeSpan.FromMinutes(5), _stopToken);
                        continue;
                    }

                    foreach (var id in listIds)
                    {
                        if (_stopToken.IsCancellationRequested) break;

                        var data = await _price.GetPrice(id);
                        if (data.finalPrice != "N/A")
                        {
                            TrackedGamesItem item = new TrackedGamesItem(id, data.finalPrice, data.discount);
                            
                            await db.UpdateWishlistItem(item);

                            _logger.LogInformation($"(LOG) >>> Game {id} updated {data.finalPrice} (-{data.discount}%)");
                        }

                        await Task.Delay(_requestSendDelay, _stopToken);
                    }

                    _logger.LogInformation($"(LOG) >>> Update finished. Next update after {_pricesUpdateDelay.TotalHours} hours");

                    await Task.Delay(_pricesUpdateDelay, _stopToken);
                }
            }
            catch (Exception ex)
            {
                if (_stopToken.IsCancellationRequested)
                    throw;

                _logger.LogError($"(LOG_ERR) >>> Error in background updating: {ex}");
                await Task.Delay(TimeSpan.FromMinutes(1), _stopToken);
            }
        } 
    }
}