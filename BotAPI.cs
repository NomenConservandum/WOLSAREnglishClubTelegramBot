using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
// using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Sensitive;
using BasicCommands;
using DBController;

namespace BotAPI {
    public class Bot {
    private static ITelegramBotClient botClient;
    private static ReceiverOptions receiverOptions = new ReceiverOptions();
    private static String inviteURL = "";
    private static Commands commands;
    private static DBApi DB = new DBApi();

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
                default: {
                    usernameTemp = "NONE";
                    msgTextTemp = "";
                    chatIdTemp = 0;
                    break;
                    }
                }
            Console.WriteLine($"There is a new message from {usernameTemp}!\nIt goes, \'{msgTextTemp}\'");
            if (usernameTemp == "NONE" && chatIdTemp == 0) { // could not parse the username
                Console.WriteLine($"ERROR: THE MESSAGE COULD NOT BE PARSED");
                return;
            }
            if (DB.findByUsername(usernameTemp).getStatus() == statuses.newcomer) { // The user is already in the DB
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
                        if(msgTextTemp == "NEGATIVE")
                            commands.sendMsg(
                                chatIdTemp,
                                "Oww, whyyy?"
                            );
                        break;
                    }
                }
            } else if (DB.findByUsername(usernameTemp).getStatus() == statuses.NONE) { // The user has met the bot for the first time
                if (DB.Add(new Users(chatIdTemp, usernameTemp, statuses.newcomer, 0, 0)))
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
        DB = new DBApi(sensitive.getDBName());

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
