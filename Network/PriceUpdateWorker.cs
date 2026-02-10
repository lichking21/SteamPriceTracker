using Microsoft.Extensions.Hosting;
using DataBaseOperator;
using DataBaseOperator.Entities;
using Microsoft.Extensions.Logging;
namespace Network;

public class PriceUpdateWorker : BackgroundService
{
    private readonly WishlistDB _wishlistDB;
    private readonly Price _price;
    private readonly ILogger<PriceUpdateWorker> _logger;

    private readonly TimeSpan _requestSendDelay = TimeSpan.FromSeconds(60);
    private readonly TimeSpan _pricesUpdateDelay = TimeSpan.FromHours(2);

    public PriceUpdateWorker(WishlistDB wishlistDB, Price price, ILogger<PriceUpdateWorker> logger)
    {
        _wishlistDB = wishlistDB;
        _price = price;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken _stopToken)
    {
        _logger.LogInformation("(LOG) >> Background price updating started");

        while (!_stopToken.IsCancellationRequested)
        {
            try
            {
                var listIds = await _wishlistDB.GetIDs();
                int count = listIds.Count;
                _logger.LogInformation($"(LOG) >> Found {count} items in wishlist.");

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
                        WishlistItem item = new WishlistItem(id, data.finalPrice, data.discount);
                        await _wishlistDB.UpdateWishlistItem(item);

                        _logger.LogInformation($"(LOG) >> Game {id} updated {data.finalPrice} (-{data.discount}%)");
                    }

                    await Task.Delay(_requestSendDelay, _stopToken);
                }

                _logger.LogInformation($"(LOG) >> Update finished. Next update after {_pricesUpdateDelay.TotalHours} hours");

                await Task.Delay(_pricesUpdateDelay, _stopToken);
            }
            catch (Exception ex)
            {
                if (_stopToken.IsCancellationRequested)
                    throw;

                _logger.LogError($"(ERR) >> Error in background updating: {ex}");
                await Task.Delay(TimeSpan.FromMinutes(1), _stopToken);
            }
        } 
    }
}