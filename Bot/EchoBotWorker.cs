using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Bot;

public class EchoBotWorker : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EchoBotWorker> _logger;

    public EchoBotWorker(IConfiguration configuration, ILogger<EchoBotWorker> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        string? token = _configuration["Telegram:BotToken"];
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogError("(LOG_ERR) >>> Telegram bot token missing. Set Telegram:BotToken in appsettings.json");
            return;
        }

        var botClient = new TelegramBotClient(token);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = new[] { UpdateType.Message }
        };

        _logger.LogInformation("(LOG) >>> Echo bot started polling");

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
        _logger.LogInformation($"(LOG) >>> Echoing message from chat {chatId}");

        await botClient.SendMessage(
            chatId: chatId,
            text: text,
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "(LOG_ERR) >>> Telegram polling error");
        return Task.CompletedTask;
    }
}
