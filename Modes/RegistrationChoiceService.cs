using DBEssentials;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

public class RegistrationChoiceService {
    bool DEBUG = true;
    public RegistrationChoiceService () { }
    // Pre-registration mode. Used when the user has a role 'newcomer.'
    public void StartService (
        Update update,
        String usernameTemp, String msgTextTemp, long chatIdTemp,
        Commands commands, DBApi RegistrationFormsDB, DBApi UsersDB
    ) {
        if (msgTextTemp.IndexOf('|') == -1) {
            commands.sendMsg(
                chatIdTemp,
                """
                Error: unknown command (it may be not available any more)
                Type /rescue for further help
                """
            );
        }
        var list = msgTextTemp.Split('|');
        String choice = list[0];
        String messageID = list[1];
        // Bot sends the InLine keyboard with the choice
        if (DEBUG) Console.WriteLine($"User {usernameTemp} has decided to register as a {choice}");
        switch (choice) {
            case "PARTICIPANT": {
                // should also add a registration form with the status: awaiting age choice
                if (UsersDB.UpdateByField(
                    new Users(chatIdTemp, usernameTemp, Statuses.ARCCustomer, Roles.NONE, 0), FieldsDB.Username, '\'' + usernameTemp + '\'')) // The users status is changed
                    return;
                if (RegistrationFormsDB.Add(new FillOutFormParticipants(usernameTemp, Genders.Male, 0, 0, Formats.Offline, proficiencyLevels.zero, "", "", 0, false, 0, "", RegistrationStatuses.AwaitingRoleChoice)))
                    return;
                // this mode won't be used anymore
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>() {
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Начать регистрацию", "NOTHING|" + messageID),
                        },
                    }
                );
                commands.updateInlineMessage(
                    chatIdTemp,
                    int.Parse(messageID),
                    "Начнём же регистрацию?",
                    inlineKeyboard
                );
                break;
            };
            case "MINISTER": {
                if (UsersDB.UpdateByField(new Users(chatIdTemp, usernameTemp, Statuses.ARCMinister, Roles.NONE, 0), FieldsDB.Username, usernameTemp)) // The users status is changed
                    return;
                // Add them to a list
                // this mode won't be used anymore
                // sends commands that will be available only in the next mode: 'ARCMinister'
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>() {
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Начать регистрацию", "NOTHING|" + messageID),
                        },
                    }
                );
                commands.updateInlineMessage(
                    chatIdTemp,
                    int.Parse(messageID),
                    "Начнём же регистрацию?",
                    inlineKeyboard
                );
                break;
            };
            default: {
                commands.sendMsg(
                    chatIdTemp,
                    """
                    Error: unknown command (it may be not available any more)
                    Type /rescue for further help
                    """
                );
                break;
            };
        }
    }
}

