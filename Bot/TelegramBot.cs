using DataBaseOperator;
using DataBaseOperator.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public class TelegramBot(IConfiguration configuration, ILogger<TelegramBot> logger, IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<TelegramBot> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? token = _configuration["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogError("(ERR) >> Telegram bot token missing. Set Telegram:BotToken in appsettings.json");
            return;
        }

        var botClient = new TelegramBotClient(token);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        _logger.LogInformation("(LOG) >> Echo bot started polling");

        botClient.StartReceiving(
            updateHandler: HandleUpdateAsync,
            errorHandler: HandlePollingErrorAsync,
            receiverOptions: receiverOptions,
            cancellationToken: stoppingToken
        );

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (TaskCanceledException)
        {
            // Expected when stopping.
        }
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message)
        {
            return;
        }

        if (update.Message?.Text is not string text)
        {
            return;
        }

        if (update.Message.From is null)
        {
            return;
        }

        UserItem user = await ValidateUser(update.Message.From);
        using var scope = _scopeFactory.CreateScope();
        var userDb = scope.ServiceProvider.GetRequiredService<UserDB>();
        switch (text)
        {
            case "/start":
                await SendWelcomeMessageAsync(botClient, user, cancellationToken);
                await userDb.SetUserState(user.ID, "hub");
                return;
            case "Add Game":
                await SendAddGameMessageAsync(botClient, user, cancellationToken);
                await userDb.SetUserState(user.ID, "add_game");
                return;
            case "My Wishlist":
                await SendMyWishlistMessageAsync(botClient, user, cancellationToken);
                return;
            case "/help":
            default:
                break;
        }
        switch (user.State)
        {
            case "hub":
                await HandleHubMessageAsync(botClient, user, text, cancellationToken);
                break;
            case "add_game":
                await HandleAddGameMessageAsync(botClient, user, text, cancellationToken);
                break;
        }
    }

    private async Task<UserItem> ValidateUser(User user)
    {
        // TODO: Probably talk about redis
        using var scope = _scopeFactory.CreateScope();
        var userDb = scope.ServiceProvider.GetRequiredService<UserDB>();

        if (await userDb.IsUserExist(user.Id))
        {
            return await userDb.GetUser(user.Id);
        }
        // TODO: Function must return UserItem
        var userItem = new UserItem
        {
            ID = user.Id,
            Name = user.FirstName,
        };

        await userDb.AddUserItem(userItem);
        return userItem;
    }

    private async Task HandleHubMessageAsync(ITelegramBotClient botClient, UserItem user, string text, CancellationToken cancellationToken)
    {
        switch (text)
        {
            default:
                await botClient.SendMessage(
                    chatId: user.ID,
                    text: "Welcome to Steam Price Tracker! Select one of the following options",
                    cancellationToken: cancellationToken
                );
                break;
        }
    }

    private async Task HandleAddGameMessageAsync(ITelegramBotClient botClient, UserItem user, string text, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var gameDb = scope.ServiceProvider.GetRequiredService<MainDB>();
        var userDb = scope.ServiceProvider.GetRequiredService<UserDB>();

        var gameList = await gameDb.ExportDataByTitle(text);
        string title = gameList[0].Title; // DOBAVIL

        switch (gameList.Count)
        {
            case 0:
                await botClient.SendMessage(
                    chatId: user.ID,
                    text: "No games found with that title",
                    cancellationToken: cancellationToken
                );
                break;
            case 1:
                var msg = await botClient.SendMessage(
                    chatId: user.ID,
                    text: $"Game found: {title}. Adding to wishlist...",
                    cancellationToken: cancellationToken
                );

                await AddGameToWishlist(user.ID, title);

                await botClient.EditMessageText(
                    chatId: user.ID,
                    messageId: msg.Id,
                    text: $"Game {title} added to wishlist.",
                    cancellationToken: cancellationToken
                );
                await userDb.SetUserState(user.ID, "hub");
                break;
            case > 1:
                var listToString = string.Join("\n", gameList.Select(game => $"• {game}"));
                await botClient.SendMessage(
                    chatId: user.ID,
                    text: $"Games found:\n{listToString}",
                    cancellationToken: cancellationToken
                );
                break;
            default:
                await botClient.SendMessage(
                    chatId: user.ID,
                    text: "Unreachable",
                    cancellationToken: cancellationToken
                );
                break;
        }
    }
    private async Task AddGameToWishlist(long userId, string gameName)
    {
        using var scope = _scopeFactory.CreateScope();
        // TODO: Remove this line
        var mainDb = scope.ServiceProvider.GetRequiredService<MainDB>();
        var userWishlistDb = scope.ServiceProvider.GetRequiredService<UserWishlistDB>();

        var gameId = await mainDb.GetGameID(gameName);
        await userWishlistDb.AddLink(userId, gameId);
    }

    private async Task SendMyWishlistMessageAsync(ITelegramBotClient botClient, UserItem user, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var userWishlistDb = scope.ServiceProvider.GetRequiredService<UserWishlistDB>();

        var games = await userWishlistDb.GetGamesFromWishlist(user.ID);
        if (games.Count == 0)
        {
            await botClient.SendMessage(
                chatId: user.ID,
                text: "Your wishlist is empty!",
                cancellationToken: cancellationToken
            );
            return;
        }

        await botClient.SendMessage(
            chatId: user.ID,
            text: $"Your wishlist contains {games.Count} games: {games}",
            cancellationToken: cancellationToken
        );
    }

    private static async Task SendWelcomeMessageAsync(ITelegramBotClient botClient, UserItem user, CancellationToken cancellationToken)
    {
        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { "Add Game", "My Wishlist" }
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };

        await botClient.SendMessage(
            chatId: user.ID,
            text: $"Welcome to the Steam Price Tracker bot {user.Name}!",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }

    private static async Task SendAddGameMessageAsync(ITelegramBotClient botClient, UserItem user, CancellationToken cancellationToken)
    {
        await botClient.SendMessage(
            chatId: user.ID,
            text: "Please enter the name of the game you want to add:",
            cancellationToken: cancellationToken
        );
    }


    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "(ERR) >> Telegram polling error");
        return Task.CompletedTask;
    }
}
