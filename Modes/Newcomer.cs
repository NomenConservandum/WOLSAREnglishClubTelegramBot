using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BasicCommands;
using DBController;
using Sensitive;

namespace NewcomerNS {
	public class Newcomer {
		bool DEBUG = true;
		public Newcomer() {}
		// Pre-registration mode. Used when the user has a role 'newcomer.'
        public void newcomerMode(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
			var list = msgTextTemp.Split('|');
			String choice = list[0];
			String messageID = list[1];
            // Bot sends the InLine keyboard with the choice
			if (DEBUG) Console.WriteLine($"User {usernameTemp} has decided to register as a {choice}");
            switch (choice) {
                case "PARTICIPANT": {
                    if (DB.Update(new Users(chatIdTemp, usernameTemp, Statuses.inregprocCustomer, Roles.NONE, 0))) // The users status is changed
                        return;
                    // this mode won't be used anymore
                    // sends commands that will be available only in the next mode: 'inregprocCustomer'
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Мужской", "1;MALE|" + messageID),                  
                            InlineKeyboardButton.WithCallbackData("Женский", "1;FEMALE|" + messageID),
                            },
                        }
                    );
                    commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
						"1 / ... \nТвой пол:",
						inlineKeyboard
					);
                    break;
                };
                case "MINISTER": {
                    if (DB.Update(new Users(chatIdTemp, usernameTemp, Statuses.inregprocMinister, Roles.NONE, 0))) // The users status is changed
                        return;
                    // this mode won't be used anymore
                    // sends commands that will be available only in the next mode
                    commands.sendMsg(
                        chatIdTemp,
                        "А теперь предлагаю тебе заполнить форму)"
                    );
                    break;
                };
                default: {
                    /*
					commands.sendMsg(
                        chatIdTemp,
                        "Error: unknown command (it may be not available any more)"
                    );
					*/
                    break;
                };
            }
        }
    }
}
