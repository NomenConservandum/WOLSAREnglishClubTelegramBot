using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Sensitive;

namespace BotAPI {

    public class Bot {
    private static ITelegramBotClient botClient;
    private static ReceiverOptions receiverOptions = new ReceiverOptions();
    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        try {
            switch (update.Type) {
                case UpdateType.Message: {
                    Console.WriteLine($"There is a new message from {update?.Message?.From?.Username}!\nIt goes, \'{update?.Message?.Text}\'");
                    return;
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    public static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken) {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
        var ErrorMessage = error switch {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
    
    // Should be used with 'await' keyword
    public System.Threading.Tasks.Task<Telegram.Bot.Types.User> getMe() {
        return botClient.GetMe();
    }
    
    public Bot() {
        Variables sensitive = new Variables();
        botClient = new TelegramBotClient(sensitive.getToken());
        receiverOptions.AllowedUpdates = new[] {
            UpdateType.Message,
            UpdateType.CallbackQuery,
        };
        receiverOptions.DropPendingUpdates = true; // we ignore all of the messages that were sent when the bot was offline
         
        var cts = new CancellationTokenSource();
        botClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions, cts.Token); // The bot is online
        }
    }
}
