//using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
//using Telegram.Bot.Polling;
//using Telegram.Bot.Exceptions;
using BasicCommands;
using DBController;
using Sensitive;

namespace BotModes {
    public class Modes {
		private Boolean DEBUG = true;
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
					int incomingMessageID = update?.Message?.Id ?? 0;
					if (msgTextTemp != "/start") {
                        commands.sendMsg(
                            chatIdTemp,
                            "Если что, команда: /start"
                        );
						commands.deleteMessage(chatIdTemp, incomingMessageID);
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
                        "🏃🏻LOADING...",
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
							var list = msgTextTemp.Split(';');
                            var inlineKeyboard = new InlineKeyboardMarkup(
                                new List<InlineKeyboardButton[]>() {
                                    new InlineKeyboardButton[] {
                                    InlineKeyboardButton.WithCallbackData("Стать участником!", "PARTICIPANT|" + list[1]),                  
                                    InlineKeyboardButton.WithCallbackData("Стать служителем!", "MINISTER|" + list[1]),
                                    },
                                }
                            );
                            // update the inline message and delete the data transfer message
                            commands.updateInlineMessage(chatIdTemp, int.Parse(list[1]), "Я хочу...", inlineKeyboard);
							commands.deleteMessage(chatIdTemp, int.Parse(list[2]));
                            if (DB.Add(new Users(chatIdTemp, usernameTemp, statuses.newcomer, roles.NONE, 0))) // The user is added to the DB
                                return; // what? The user is already in the DB? Then they shouldn't get here.
							if (DEBUG) Console.WriteLine($"User {usernameTemp}: new status \'Newcomer\'");
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
			var list = msgTextTemp.Split('|');
			String choice = list[0];
			String messageID = list[1];
            // Bot sends the InLine keyboard with the choice
			if (DEBUG) Console.WriteLine($"User {usernameTemp} has decided to register as a {choice}");
            switch (choice) {
                case "PARTICIPANT": {
                    if (DB.Update(new Users(chatIdTemp, usernameTemp, statuses.inregprocCustomer, roles.NONE, 0))) // The users status is changed
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
						"1 / ... .\nТвой пол:",
						inlineKeyboard
					);
                    break;
                };
                case "MINISTER": {
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
        public void inregprocCustomer(
            Update update,
            String usernameTemp, String msgTextTemp, long chatIdTemp,
            Commands commands, DBApi DB
        ) {
			if (msgTextTemp.Split('|').Count() == 1) {
				// FIX: Это момент, когда пользователь сам пишет своё предложение о мессенджере или месте проведения.
				// Я не знаю пока как это обрабатывать, скорее всего придётся от этого просто отказать и заместо
				// "Свой вариант" писать "Другое" и никак это не обрабатывать и пусть хосты разбираются со всеми сами
				// NOTE: можно кодировать строку с выборами и отсылать прямо в сообщении, а после брать это сообщение, вынимать строку, дэкодировать и норм (только как мне достать сообщение тогда?)
				return;
			}
			var list = msgTextTemp.Split('|');
			var bodyList = list[0].Split(';');
			char stage = bodyList[0][0];
			String messageID = list[1];
            String tempBody;
			// if (DEBUG) Console.WriteLine($"User {usernameTemp} is at stage {stage}");
			switch (stage) {
				case '1': {
					if (DEBUG) Console.WriteLine($"User {usernameTemp} is {bodyList[1]}");
					tempBody = "2;" + bodyList[1]; // guarantees the next stage. Now this variable is used as a temporary string.
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("15", tempBody + ";15|" + messageID),
                            InlineKeyboardButton.WithCallbackData("16", tempBody + ";16|" + messageID),
                            InlineKeyboardButton.WithCallbackData("17", tempBody + ";17|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("18", tempBody + ";18|" + messageID),
                            InlineKeyboardButton.WithCallbackData("19", tempBody + ";19|" + messageID),
                            InlineKeyboardButton.WithCallbackData("20", tempBody + ";20|" + messageID),
                            InlineKeyboardButton.WithCallbackData("21", tempBody + ";21|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("22", tempBody + ";22|" + messageID),
                            InlineKeyboardButton.WithCallbackData("23", tempBody + ";23|" + messageID),
                            InlineKeyboardButton.WithCallbackData("24", tempBody + ";24|" + messageID),
                            InlineKeyboardButton.WithCallbackData("25", tempBody + ";25|" + messageID),
                            },
                        }
                    );
                    commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
                        "2 / ... .\nТвой возраст:",
                        inlineKeyboard
					);
                    // sends commands that will be available only on the next stage
                    break;
                };
				case '2': {
					tempBody = "3;" + bodyList[1] + ';' + bodyList[2]; // guarantees the next stage. Now this variable is used as a temporary string.
					if (DEBUG) Console.WriteLine($"User {usernameTemp} is {bodyList[2]} years old");
                    // sends commands that will be available only on the next stage
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Нулевой: Знаю пару слов максимум😜", tempBody + ";0|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("A1: Могу делать простые предложения🎩", tempBody + ";A1|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("A2: Могу рассказать, что делаю по жизни", tempBody + ";A2|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("B1: Могу легко делиться мнением / хобби", tempBody + ";B1|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("B2: Могу легко общаться на разные темы", tempBody + ";B2|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("C1: Могу изъясняться легко и спонтанно💅", tempBody + ";C1|" + messageID),
                            },
                        }
                    );
                    commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
                        "3 / ... .\nТвой уровень владения языком:",
                        inlineKeyboard
					);
                    break;
                };
				case '3': {
                    Console.WriteLine($"User {usernameTemp} has {bodyList[3]} level knowledge about English.");
					tempBody = "4;" + bodyList[1] + ';' + bodyList[2] + ';' + bodyList[3]; // guarantees the next stage. Now this variable is used as a temporary string.
                    var inlineKeyboard = new InlineKeyboardMarkup(
                        new List<InlineKeyboardButton[]>() {
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Оффлайн", tempBody + ";OFFLINE|" + messageID),
                            },
                            new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Онлайн", tempBody + ";ONLINE|" + messageID),
                            },
                        }
                    );
                    commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
                        "4 / ... .\nКакого формата предпочтёшь встречи?",
                        inlineKeyboard
					);
					break;
				}
				case '4': {
					//
					tempBody = ";" + bodyList[1] + ';' + bodyList[2] + ';' + bodyList[3] + ';' + bodyList[4] + ":"; // Does NOT guarantee the next stage. Now this variable is used as a temporary string.
                    Console.WriteLine($"User {usernameTemp} has chosen {bodyList[4]} meetings.");
					String[] optionText = {"", "", ""}, optionCode = {"", "", ""}; // I'm very sorry for this dumb and straightforward code, I don't know how to do better
					String msgText = "";
					switch (bodyList[4]) {
						case "OFFLINE": {
							msgText = "Твой выбор: оффлайн встречи.\nГде ты предпочтёшь встречаться?";
							optionText[0] = "В здании церкви"; // TODO: Добавить адрес при получении разрешения
							optionCode[0] = "CHURCH";
							//optionText[2] = "в здании СГУ (12й корпус)"; // NOTE: Я не знаю, буду ли это добавлять
							//optionCode[2] = "SSU12D";
							optionText[1] = "Тайм-кафе (дружба, лофт и другие)";
							optionCode[1] = "TIME-CAFE";
							optionText[2] = "TEMPORARY UNAVAILABLE"; // FIX: Заменить либо на СГУ, либо на что-то другое нейтральное
							optionCode[2] = "NONE";
							break;
						}
						case "ONLINE": {
							msgText = "Твой выбор: онлайн встречи.\nГде ты предпочтёшь созваниваться?";
							optionText[0] = "в Telegram";
							optionCode[0] = "TELEGRAM";
							optionText[1] = "в Discord"; 
							optionCode[1] = "DISCORD";
							optionText[2] = "в VK";
							optionCode[2] = "VK";
							break;
					    }
						// In these two cases the bot asks the user to write their own idea (sends a message and saves its id) (less than 20 symbols)
						case "OFFLINE:OTHER": {
							var msg = commands.sendMsg(
									chatIdTemp,
									"Напиши свой вариант (Формат сообщения: не более 20 символов)"
								);
							break;
						}
						case "ONLINE:OTHER": {
							var msg = commands.sendMsg(
									chatIdTemp,
									"Напиши свой вариант (Формат сообщения: не более 20 символов)"
								);
							break;
						}
					}

					String BaseString = '5' + tempBody + "CODE|" + messageID;
			        
					var inlineKeyboardList =
						new List<InlineKeyboardButton[]>() {
		                    new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("...", "NONE"),
				            },
		                    new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("...", "NONE"),
				            },
		                    new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("...", "NONE"),
				            },
					        new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData("Свой вариант (Баг: обнуляет прогресс)", '4' + tempBody + "OTHER|" + messageID),
							},
					        //new InlineKeyboardButton[] {
							//InlineKeyboardButton.WithCallbackData("Следующий вопрос", tempBody + "|" + messageID),
							//},
						};
					
					for (int i = 0; i < 3; ++i)
					    inlineKeyboardList[i][0] =
							InlineKeyboardButton.WithCallbackData(
								optionText[i],
								BaseString.Replace("CODE", optionCode[i])
							);
		            
		            var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardList);
					
					commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
						msgText,
				        inlineKeyboard
					);
					break;
				}
				case '5': {
					String msgText = "5 / ...: Что хочешь видеть на встречах клуба?";
					String[][] options = new String[][] {
						new String [] {"Настольные игры", "BOARDGAMES"},
						new String [] {"Видеоигры", "VIDEOGAMES"},
						new String [] {"Книги", "BOOKS"},
						new String [] {"ИБДН", "BSFNR"},
						new String [] {"Еда", "FOOD"},
						new String [] {"Структурированный материал", "STRUCTUREDMATERIALS"},
						new String [] {"Свой вариант (ложит процесс)", "CUSTOM"}, // потенциальное решение: закэшировать весь код и отправить его в следующем сообщении бота, дождаться ответа юзера и после удалить оба сообщения, декэшировать и вернуть выбор
						new String [] {"Следующий вопрос", "NEXT"}
					};
					// I'm very sorry for this dumb and straightforward code, I don't know how to do better
					short numberOfOptions = 7, chosenMask = 0;
					if (bodyList.Count() == 6) { // The user is on this stage not for the first time
						chosenMask = short.Parse(bodyList[5]);
						if (chosenMask != 0) numberOfOptions = 8; // now we include the 'next question' button
						if (bodyList.Count() == 7) {// The user is on this stage not for the first time
							msgText += "\nТвой выбор: ";
							for (int i = 0; i < 7; ++i)
								if (((chosenMask >> i) & 1) == 1) // the option is chosen
									msgText += ", " + options[i][0];
						}
					}
					// it will have many interest options
					var inlineKeyboardList = new List<InlineKeyboardButton[]>() {};
					
					for (short i = 0; i < numberOfOptions; ++i) {
						String resultString = "";
						if (((chosenMask >> i) & 1) == 0 && i != 7) // check if the user hasn't chosen this particular option
							resultString += "5;";
						if (i == 7) { // let's check for the 'next' button first
							resultString += "6;";
						}
						short tempMask = chosenMask;
						// now let's make a proper string: if it was chosen already, it should be removed, if not, it should be added
						if (((chosenMask >> i) & 1) == 1) { // the option is chosen and add the 'Убрать выбор' text
							tempMask &= (short)~(1 << i); // we remove the option as being chosen from the mask for the particular button
							options[i][0] += " (убрать выбор)";
							resultString += "5;"; // HACK: I guess this system is absolete: I can just split the string and look how many parts are there.
						} else { // add the option
							tempMask |= (short)(1 << i); // we add the option to the mask for the particular button
						}
						// merge the body into one string
						for (int j = 1; j < 5; ++j)
							resultString += bodyList[j] + ';';
						resultString += tempMask.ToString() + '|' + messageID;
						
						inlineKeyboardList.Add(
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData(options[i][0], resultString)
							}
						);
					}
					
		            var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardList);
					
					commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
						msgText,
				        inlineKeyboard
					);
					break;
				}
                default: {
                    commands.sendMsg(
                        chatIdTemp,
                        "Error: unknown command (it may be not available any more)"
                    );
                    break;
                };
            }
		}
		//
    }
}
