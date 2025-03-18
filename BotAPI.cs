using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Sensitive;
using BasicCommands;

namespace BotAPI {
    public class Bot {
    private static ITelegramBotClient botClient;
    private static ReceiverOptions receiverOptions = new ReceiverOptions();
    private static String inviteURL = "";
    private static Commands commands;
    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        try {
            switch (update.Type) {
                case UpdateType.Message: {
                    Console.WriteLine($"There is a new message from {update?.Message?.From?.Username}!\nIt goes, \'{update?.Message?.Text}\'");
                    commands.sendInlineURL(
                        update.Message.Chat.Id,
                        "I\'d like to invite you to our chat!\nThe link is here:", 
                        "Your link :D",
                        "No, thank you",
                        inviteURL
                    );
                    
                    return;
                }
            }
        }
        catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }

    // Not my code, lol
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
    // Not my code block ends

    // Should be used with 'await' keyword
    public System.Threading.Tasks.Task<Telegram.Bot.Types.User> getMe() {
        return botClient.GetMe();
    }
    
    public Bot() {
        Variables sensitive = new Variables();
        inviteURL = sensitive.getURL();
        botClient = new TelegramBotClient(sensitive.getToken());
        commands = new Commands(botClient);
        
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
