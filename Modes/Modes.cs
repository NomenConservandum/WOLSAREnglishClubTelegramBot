using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using BasicCommands;
using DBController;
using Sensitive;
using firstEncounterNS;
using NewcomerNS;
using RegistrationProcessNS;

namespace BotModes {
	// The 'Hub' for the modes. Each mode is now moved to a separate file for redacting purposes
    public class Modes {
        public Modes() {
            // ...
        }
        public void FillerMode(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB,
            String inviteURL
        ) {
                switch (update.Type) {
                    case UpdateType.Message: { // Ask them if they want to register and if so, what role would they like to have: a minister, a participant
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
                        switch (msgTextTemp) {
                            case "NEGATIVE": {
                                commands.sendMsg(
                                    chatIdTemp,
                                    "Oww, whyyy?"
                                );
                                break;
                            };
                            default: {
                                commands.sendMsg(
                                    chatIdTemp,
                                    "Error: unknown command (it may be not available any more)"
                                );
                                break;
                            }
                        }
                        break;
                    }
                }
        }
        public void FirstEncounter(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
            firstEncounter mode = new firstEncounter();
            mode.FirstEncounter(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
        }

        // other modes
        
        // Pre-registration mode. Used when the user has a role 'newcomer.'
        public void newcomerMode(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
			Newcomer mode = new Newcomer();
			mode.newcomerMode(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
        }
        public void inregprocCustomer(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
			RegistrationProcess mode = new RegistrationProcess();
			mode.Customer(update, usernameTemp, msgTextTemp, chatIdTemp, commands, DB);
		}
		//
    }
}
