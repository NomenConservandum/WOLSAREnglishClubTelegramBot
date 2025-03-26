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
                    if (msgTextTemp != "/start") {
                        commands.sendMsg(
                            chatIdTemp,
                            "Если что, команда: /start"
                        );
                        break;
                    }
                    Variables sensitive = new Variables();
                    // Bot sends the InLine keyboard with the choice
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Начать регистрацию!", "REGISTRATION"),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithUrl("Узнать больше о клубе (FAQ)", sensitive.getFAQLink()), 
                            },
                        }
                    );

                    commands.sendMsgInline(
                        chatIdTemp,
                        "Я хочу...",
                        inlineKeyboard
                    );
                    var dataTransferAgreenment = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithUrl("Согласие на обработку персональных данных", sensitive.getDTALink()),
                            },
                        }
                    );
                    commands.sendMsgInline(
                        chatIdTemp,
                        "Нажимая на 'Начать регистрацию!', Вы соглашаетесь на обработку персональных данных (текст согласия доступен ниже)",
                        dataTransferAgreenment
                    );
                    
                    
                    break;
                };
                case UpdateType.CallbackQuery: {
                    switch (msgTextTemp) {
                        case "REGISTRATION": {
                            var inlineKeyboard = new InlineKeyboardMarkup(
                                new List<InlineKeyboardButton[]>() {
                                    new InlineKeyboardButton[] {
                                    InlineKeyboardButton.WithCallbackData("Стать участником!", "PARTICIPANT"),                  
                                    InlineKeyboardButton.WithCallbackData("Стать служителем!", "MINISTER"),
                                    },
                                }
                            );
                            // send a message with the inline keyboard
                            commands.sendMsgInline(
                                chatIdTemp,
                                "Я хочу...",
                                inlineKeyboard
                            );
                            if (DB.Add(new Users(chatIdTemp, usernameTemp, statuses.newcomer, roles.NONE, 0))) // The user is added to the DB
                                return; // what? The user is already in the DB? Then they shouldn't get here.
                            // switches the mode by adding the user to the DB
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
        
        // Pre-registration mode. Used when the user has a role 'newcomer.'
        public void newcomerMode(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
            // Bot sends the InLine keyboard with the choice
            switch (msgTextTemp) {
                case "PARTICIPANT": {
                    Console.WriteLine($"The user {usernameTemp} wants to register as a participant!");
                    if (DB.Update(new Users(chatIdTemp, usernameTemp, statuses.inregprocCustomer, roles.NONE, 0))) // The users status is changed
                        return;
                    // this mode won't be used anymore
                    // sends commands that will be available only in the next mode
                    break;
                };
                case "MINISTER": {
                    Console.WriteLine($"The user {usernameTemp} wants to register as a minister!");
                    if (DB.Update(new Users(chatIdTemp, usernameTemp, statuses.inregprocMinister, roles.NONE, 0))) // The users status is changed
                        return;
                    // this mode won't be used anymore
                    // sends commands that will be available only in the next mode
                    break;
                };
                default: {
                    commands.sendMsg(
                        chatIdTemp,
                        "Error: unknown command (it may be not available any more)"
                    );
                    break;
                };
            }
        }
    }
}
