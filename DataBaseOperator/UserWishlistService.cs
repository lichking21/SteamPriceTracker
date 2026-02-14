using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Network;

namespace DataBaseOperator;

public class DBService
{
    private MainDB _mainDB;
    private TrackedGamesDB _trackedGamesDB;
    private UserWishlistDB _userWishlistDB;
    private Price _price;
    private readonly ILogger<DBService> _logger;

    public DBService(MainDB mainDB, TrackedGamesDB trackedGamesDB, UserWishlistDB userWishlistDB, 
                    ILogger<DBService> logger, Price price)
    {
        _mainDB = mainDB;
        _trackedGamesDB = trackedGamesDB;
        _userWishlistDB = userWishlistDB;
        _logger = logger;
        _price = price;
    } 

    public async Task AddToUserWishlist(long userId, string gameTitle)
    {
        int gameId = await _mainDB.GetGameID(gameTitle);
        if (gameId == 0)
        {
            _logger.LogError($"(ERR) >> Game: {gameTitle} wasn't found");
            return;
        }

        bool isTracked = await _trackedGamesDB.IsTracking(gameId);
        if (isTracked == false)
        {
            (string finalPrice, int discount) = await _price.GetPrice(gameId);

            var newTrackItem = new TrackedGamesItem(gameId, finalPrice, discount, gameTitle);
            
            await _trackedGamesDB.AddTrackingGame(newTrackItem);
        }

        bool added = await _userWishlistDB.AddLink(userId, gameId);

        if (added)
            _logger.LogInformation($"(LOG) >> {gameTitle} was added to your wishlist");
        else
            _logger.LogWarning($"(WARN) >> {gameTitle} is already in your wishlist");
    }
}