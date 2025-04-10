using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using Sensitive;
using BasicCommands;
using DBController;
using BotModes;

namespace BotAPI {
    public class Bot {
    private static ITelegramBotClient? botClient;
    private static ReceiverOptions receiverOptions = new ReceiverOptions();
    private static String inviteURL = "";
    private static Commands commands = new Commands();
    private static DBApi DB = new DBApi();
    private static Modes modes = new Modes();
    public static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken) {
        String usernameTemp = "", msgTextTemp = "";
        long chatIdTemp = 0;
        {
            String[] data = commands.getDataFromUpdate(update);
            usernameTemp = data[0];
            if (usernameTemp == " ") { // could not parse the username, no need for the further check
                Console.WriteLine($"ERROR: the username could not be parsed! Update converted to string: {update.ToString()}");
                return;
            }
            msgTextTemp = data[1];
            chatIdTemp = long.Parse(data[2]);
        }
        try {
            Console.WriteLine($"User {usernameTemp}: \"{msgTextTemp}\" ");
            Users foundUser = DB.findByUsername(usernameTemp);
            if (foundUser.getStatus() == Statuses.newcomer) { // The user is already in the DB
                modes.newcomerMode(
                    update,
                    usernameTemp, msgTextTemp, chatIdTemp,
                    commands, DB
                );
            } else if (foundUser.getStatus() == Statuses.inregprocCustomer) {
                // Console.WriteLine($"User {usernameTemp}: registering as a participant");
				modes.inregprocCustomer(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
			} else if (foundUser.getStatus() == Statuses.inregprocMinister) {
                // Nothing here yet
                // Console.WriteLine($"User {usernameTemp}: registering as a minister");
            }
            else if (foundUser.isValid() == false) { // The user has met the bot for the first time
                // The 'first encounter' mode
                modes.FirstEncounter(
                    update,
                    usernameTemp, msgTextTemp, chatIdTemp,
                    commands, DB
                );
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
	
	public User? getMe() {
		return botClient?.GetMe().Result;
	}

    // Should be used with 'await' keyword
    //public System.Threading.Tasks.Task<Telegram.Bot.Types.User> getMe() {
    //    return botClient?.GetMe() ?? new User();
    //}
    
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
