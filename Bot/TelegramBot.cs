using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace Bot;

// public class TelegramBot : BackgroundService
// {
//     private readonly TelegramBotClient _botClient;

//     public TelegramBot(string token)
//     {
//         _botClient = new TelegramBotClient(token);
//     }

//     public async Task SendMessageAsync(long chatId, string text)
//     {
//         await _botClient.SendTextMessageAsync(chatId, text);
//     }
// }
