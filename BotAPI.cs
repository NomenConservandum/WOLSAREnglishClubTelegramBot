using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
// using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Sensitive;
using BasicCommands;
using BDBC;

namespace BotAPI {
    public class Bot {
    private static ITelegramBotClient botClient;
    private static ReceiverOptions receiverOptions = new ReceiverOptions();
    private static String inviteURL = "";
    private static Commands commands;
    private static PseudoDB DB = new PseudoDB();

    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        try {
            String usernameTemp = "", msgTextTemp = "";
            long chatIdTemp = 0;
            
            switch (update.Type) { // to get the username, text of the message and the chat ID
                case UpdateType.Message: {
                    usernameTemp = update.Message.From.Username;
                    msgTextTemp = update.Message.Text;
                    chatIdTemp = update.Message.Chat.Id;
                    break;
                }
                case UpdateType.CallbackQuery: {
                    usernameTemp = update.CallbackQuery.From.Username;
                    msgTextTemp = update.CallbackQuery.Data;
                    chatIdTemp = update.CallbackQuery.Message.Chat.Id;
                    break;
                }
            }
            Console.WriteLine($"There is a new message from {usernameTemp}!\nIt goes, \'{msgTextTemp}\'");
            
            if (DB.findByUsername(usernameTemp).isValid()) { // The user is already in the DB
                switch (update.Type) {
                    case UpdateType.Message: {
                        commands.sendInlineURL(
                            chatIdTemp,
                            "I\'d like to invite you to our chat!\nThe link is here:", 
                            "Your link :D",
                            "No, thank you",
                            inviteURL
                        );
                        break;
                    }
                    case UpdateType.CallbackQuery: {
                        if(msgTextTemp == "NEGATIVE") // delete the inline message.
                            commands.sendMsg(
                                chatIdTemp,
                                "Oww, whyyy?"
                            );
                        break;
                    }
                }
            } else { // The user has met the bot for the first time
                if(DB.Add(new Users(usernameTemp)))
                    return;
                switch(update.Type) {
                    default: {
                        commands.sendMsg(
                            chatIdTemp,
                            "You're a newbie! :D"
                        );
                        break;
                    };
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
