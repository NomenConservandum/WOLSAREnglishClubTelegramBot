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
                            InlineKeyboardButton.WithCallbackData("...", "NONE"),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("...", "NONE"), 
                            },
                        }
                    );
                    var msg1 = commands.sendMsgInline(
                        chatIdTemp,
                        "LOADING...",
                        inlineKeyboard
                    );
                    var dataTransferAgreenment = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithUrl("Согласие на обработку персональных данных", sensitive.getDTALink()),
                            },
                        }
                    );
                    var msg2 = commands.sendMsgInline(
                        chatIdTemp,
                        "Нажимая на 'Начать регистрацию!', Вы соглашаетесь на обработку персональных данных (текст согласия доступен ниже)",
                        dataTransferAgreenment
                    );
					String dataString = "REGISTRATION;" + msg1.Id.ToString() + ";" + msg2.Id.ToString();
                    inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Начать регистрацию!", dataString)
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithUrl("Узнать больше о клубе (FAQ)", sensitive.getFAQLink()), 
                            },
                        }
                    );
                    
					// update the registration button to have ID's inside to pass them further
                    commands.updateInlineMessage(chatIdTemp, msg1.Id, "Я хочу...", inlineKeyboard);
					
                    break;
                };
                case UpdateType.CallbackQuery: {
                    switch (msgTextTemp.Substring(0, 12)) {
                        case "REGISTRATION": {
                            var inlineKeyboard = new InlineKeyboardMarkup(
                                new List<InlineKeyboardButton[]>() {
                                    new InlineKeyboardButton[] {
                                    InlineKeyboardButton.WithCallbackData("Стать участником!", "PARTICIPANT"),                  
                                    InlineKeyboardButton.WithCallbackData("Стать служителем!", "MINISTER"),
                                    },
                                }
                            );
							var list = msgTextTemp.Split(';');
                            // update the inline message and delete the data transfer message
                            commands.updateInlineMessage(chatIdTemp, int.Parse(list[1]), "Я хочу...", inlineKeyboard);
							commands.deleteMessage(chatIdTemp, int.Parse(list[2]));
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
                    // sends commands that will be available only in the next mode: 'inregprocCustomer'
                    var msg1 = commands.sendMsg(
                        chatIdTemp,
                        "А теперь предлагаю тебе заполнить форму)"
                    );
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Мужской", "1;MALE"),                  
                            InlineKeyboardButton.WithCallbackData("Женский", "1;FEMALE"),
                            },
                        }
                    );
                    // send a message with the inline keyboard
                    commands.sendMsgInline(
                        chatIdTemp,
                        "Первый вопрос.\nТвой пол:",
                        inlineKeyboard
                    );
                    break;
                };
                case "MINISTER": {
                    Console.WriteLine($"The user {usernameTemp} wants to register as a minister!");
                    if (DB.Update(new Users(chatIdTemp, usernameTemp, statuses.inregprocMinister, roles.NONE, 0))) // The users status is changed
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
                    commands.sendMsg(
                        chatIdTemp,
                        "Error: unknown command (it may be not available any more)"
                    );
                    break;
                };
            }
        }
        public void inregprocCustomer(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
			var bodyList = msgTextTemp.Split(';');
            String tempBody;
			switch (bodyList[0]) {
				case "1": {
                    Console.WriteLine($"{usernameTemp} is " + bodyList[1]);
					tempBody = "2;" + bodyList[1]; // guarantees the next stage. Now this variable is used as a temporary string.
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("15", tempBody + ";15"),
                            InlineKeyboardButton.WithCallbackData("16", tempBody + ";16"),
                            InlineKeyboardButton.WithCallbackData("17", tempBody + ";17"),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("18", tempBody + ";18"),
                            InlineKeyboardButton.WithCallbackData("19", tempBody + ";19"),
                            InlineKeyboardButton.WithCallbackData("20", tempBody + ";20"),
                            InlineKeyboardButton.WithCallbackData("21", tempBody + ";21"),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("22", tempBody + ";22"),
                            InlineKeyboardButton.WithCallbackData("23", tempBody + ";23"),
                            InlineKeyboardButton.WithCallbackData("24", tempBody + ";24"),
                            InlineKeyboardButton.WithCallbackData("25", tempBody + ";25"),
                            },
                        }
                    );
                    // send a message with the inline keyboard
                    commands.sendMsgInline(
                        chatIdTemp,
                        "Второй вопрос.\nТвой возраст:",
                        inlineKeyboard
                    );

                    // sends commands that will be available only on the next stage
                    break;
                };
				case "2": {
					int age = int.Parse(bodyList[2]);
                    Console.WriteLine($"The user {usernameTemp} is " + age.ToString() + " years old.");
                    // sends commands that will be available only on the next stage
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
