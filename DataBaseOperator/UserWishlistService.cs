using DataBaseOperator.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Network;

namespace DataBaseOperator;

public class UserWishlistService
{
    private MainDB _mainDB;
    private TrackedGamesDB _trackedGamesDB;
    private UserDB _userDB;
    private UserWishlistDB _userWishlistDB;
    private Price _price;
    private readonly ILogger<UserWishlistService> _logger;

    public UserWishlistService(MainDB mainDB, TrackedGamesDB trackedGamesDB, UserWishlistDB userWishlistDB, 
                    UserDB userDB, ILogger<UserWishlistService> logger, Price price)
    {
        _mainDB = mainDB;
        _trackedGamesDB = trackedGamesDB;
        _userWishlistDB = userWishlistDB;
        _userDB = userDB;

        _logger = logger;
        _price = price;
    } 

    /// <summary>
    /// Adds game to user's wishlist by it's title 
    /// </summary>
    public async Task AddByID(long userId, int gameId)
    {
        string gameTitle = await _mainDB.GetGameTitle(gameId);
        if (string.IsNullOrEmpty(gameTitle))
        {
            _logger.LogError($"(ERR) >> Game with ID [{gameId}] not found");
            return;
        }

        var userRegion = await _userDB.GetUserRegion(userId);
        if (string.IsNullOrEmpty(userRegion))
        {
            _logger.LogError("(ERR) >> User region is null or empty.");
            return;
        }

        bool isTracked = await _trackedGamesDB.IsTracking(gameId, userRegion);
        if (isTracked == false)
        {
            (string finalPrice, int discount) = await _price.GetPrice(gameId, userRegion);
            var newTrackItem = new TrackedGamesItem(gameId, userRegion, finalPrice, discount, gameTitle);
            await _trackedGamesDB.AddTrackingGame(newTrackItem);
        }

        bool added = await _userWishlistDB.AddLink(userId, gameId);
        if (added)
            _logger.LogInformation($"(LOG) >> Game [{gameTitle}] was added to your wishlist");
        else
            _logger.LogWarning($"(WARN) >> Game [{gameTitle}] is already in your wishlist");
    }

    /// <summary>
    /// Adds game to user's wishlist by it's title
    /// </summary>
    public async Task AddByTitle(long userId, string gameTitle)
    {
        int gameId = await _mainDB.GetGameID(gameTitle);
        if (gameId == 0)
        {
            _logger.LogError($"(ERR) >> Game [{gameTitle}] not found");
            return;
        }

        await AddByID(userId, gameId);
    }
}