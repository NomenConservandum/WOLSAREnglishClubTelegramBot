using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;
using BasicCommands;
using DBController;
using Sensitive;

namespace BotModes {
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
            switch(update.Type) {
                case UpdateType.Message: { // greet the user and suggest them to go through a regestration process
                    if (update.Message.Text != "/start") {
                        commands.sendMsg(
                            chatIdTemp,
                            "Если что, команда: /start"
                        );
                        break;
                    }
                    // Bot sends the InLine keyboard with the choice
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Стать участником!", "PARTICIPANT"),
                            InlineKeyboardButton.WithCallbackData("Стать служителем!", "MINISTER"),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Узнать больше о клубе", "INFO"), 
                            },
                        }
                    );
                    commands.sendMsgInline(
                        chatIdTemp,
                        "Я хочу...",
                        inlineKeyboard
                    );
                    break;
                };
                case UpdateType.CallbackQuery: {
                    switch (msgTextTemp) {
                        case "PARTICIPANT": {
                            Console.WriteLine($"The user {usernameTemp} wants to register as a participant!");
                            // The user is added to the DB
                            if (DB.Add(new Users(chatIdTemp, usernameTemp, statuses.newcomer, roles.NONE, 0, proficiencyLevels.zero)))
                                return;
                            break;
                        };
                        case "MINISTER": {
                            Console.WriteLine($"The user {usernameTemp} wants to register as a minister!");
                            if (DB.Add(new Users(chatIdTemp, usernameTemp, statuses.newcomer, roles.NONE, 0, proficiencyLevels.zero)))
                            // The user is added to the DB
                                return;
                            break;
                        };
                        case "INFO": {
                            Console.WriteLine($"The user {usernameTemp} wants to know more about the club!");
                            break;
                        }
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
        // other modes
    }
}
