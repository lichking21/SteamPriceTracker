using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Bot;

public class TelegramBot(IConfiguration configuration, ILogger<TelegramBot> logger) : BackgroundService
{
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

        var chatId = update.Message.Chat.Id;
        _logger.LogInformation($"(LOG) >> Message from {chatId}");
        switch (text)
        {
            case "/start":
                await SendWelcomeMessageAsync(botClient, chatId, cancellationToken);
                break;
            case "/help":
            default:
                break;
        }
    }

    private static async Task SendWelcomeMessageAsync(ITelegramBotClient botClient, long chatId, CancellationToken cancellationToken)
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
            chatId: chatId,
            text: "Welcome to the Steam Price Tracker bot!",
            replyMarkup: keyboard,
            cancellationToken: cancellationToken
        );
    }


    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "(ERR) >> Telegram polling error");
        return Task.CompletedTask;
    }
}
