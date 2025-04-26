using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using DBEssentials;

public class RegistrationProcessService {
	bool DEBUG = true;
	public RegistrationProcessService () { }
	public void StartServiceCustomer (
				Update update,
				String usernameTemp, String msgTextTemp, long chatIdTemp,
				Commands commands, DBApi DB
			) {
		if (msgTextTemp.Split('|').Count() == 1) {
			return;
		}
		var list = msgTextTemp.Split('|');
		var bodyList = list[0].Split(';');
		char stage = bodyList[0][0];
		String messageID = list[1];
		String tempBody;
		switch (stage) {
			case '1': {
					if (DEBUG) Console.WriteLine($"User {usernameTemp} is {bodyList[1]}");
					tempBody = '2' + commands.mainBodyOnly(bodyList); // guarantees the next stage. Now this variable is used as a temporary string.
					var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };
					for (short i = 15; i < 26; i += 4) {
						inlineKeyboardList.Add(
							[
								InlineKeyboardButton.WithCallbackData(i.ToString(), tempBody + ';' + i.ToString() + '|' + messageID),
								InlineKeyboardButton.WithCallbackData((i + 1).ToString(), tempBody + ';' + (i + 1).ToString() + '|' + messageID),
								InlineKeyboardButton.WithCallbackData((i + 2).ToString(), tempBody + ';' + (i + 2).ToString() + '|' + messageID),
								InlineKeyboardButton.WithCallbackData((i + 3).ToString(), tempBody + ';' + (i + 3).ToString() + '|' + messageID)
							]
						);
					}
					var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardList);
					commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
						"2 / ... \nТвой возраст:",
						inlineKeyboard
					);
					// sends commands that will be available only on the next stage
					break;
				}
				;
			case '2': {
					tempBody = '3' + commands.mainBodyOnly(bodyList); // guarantees the next stage. Now this variable is used as a temporary string.
					if (DEBUG) Console.WriteLine($"User {usernameTemp} is {bodyList[2]} years old");
					// sends commands that will be available only on the next stage
					String[] levelsDescription =
                        [
							"Нулевой: Знаю пару слов максимум😜",
							"A1: Могу делать простые предложения🎩",
							"A2: Могу рассказать, что делаю по жизни",
							"B1: Могу легко делиться мнением / хобби",
							"B2: Могу легко общаться на разные темы",
							"C1: Могу изъясняться легко и спонтанно💅",
						];
					var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };
					for (int i = 0; i < 6; ++i)
						inlineKeyboardList.Add(
							[InlineKeyboardButton.WithCallbackData(levelsDescription[i] , tempBody + ';' + i.ToString() + '|' + messageID)]
						);
					var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardList);
					commands.updateInlineMessage(
						chatIdTemp,
						int.Parse(messageID),
						"3 / ... \nТвой уровень владения языком:",
						inlineKeyboard
					);
					break;
				}
				;
			case '3': {
					Console.WriteLine($"User {usernameTemp} has {(proficiencyLevels)short.Parse(bodyList[3])} level knowledge about English.");
					tempBody = '4' + commands.mainBodyOnly(bodyList); // guarantees the next stage. Now this variable is used as a temporary string.
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
						"4 / ... \nКакого формата предпочтёшь встречи?",
						inlineKeyboard
					);
					break;
				}
			case '4': {
					tempBody = commands.mainBodyOnly(bodyList); // Does NOT guarantee the next stage. Now this variable is used as a temporary string.
					Console.WriteLine($"User {usernameTemp} has chosen {bodyList[4]} meetings.");
					// the first one is the visible text, the second - its code
					String[][] options = [
							["", ""],
                            ["", ""],
                            ["", ""],
							["Свой вариант (Баг: обнуляет прогресс)", '4' + tempBody + "OTHER|" + messageID],
						];
					String msgText = "";
					switch (bodyList[4]) {
						case "OFFLINE": {
								msgText = "4 / ... \nТвой выбор: оффлайн встречи.\nГде ты предпочтёшь встречаться?";
								options[0] = new String[] { "В здании церкви", "CHURCH" }; // TODO: Добавить адрес при получении разрешения
								options[1] = new String[] { "Тайм-кафе (дружба, лофт и другие)", "TIME-CAFE" };
								options[2] = new String[] { "TEMPORARY UNAVAILABLE", "NONE" }; // FIX: Заменить либо на СГУ, либо на что-то другое нейтральное
								break;
							}
						case "ONLINE": {
								msgText = "Твой выбор: онлайн встречи.\nГде ты предпочтёшь созваниваться?";
								options[0] = new String[] { "в Telegram", "TELEGRAM" };
								options[1] = new String[] { "в Discord", "DISCORD" };
								options[2] = new String[] { "в VK", "VK" };
								break;
							}
						// In these two cases the bot asks the user to write their own idea (sends a message and saves its id) (less than 20 symbols)
						// FIX: these two don't work at the moment
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

					String BaseString = '5' + tempBody + ":CODE|" + messageID;

					var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };

					for (int i = 0; i < 3; ++i)
						inlineKeyboardList.Add(
							new InlineKeyboardButton[] {
										InlineKeyboardButton.WithCallbackData(
											options[i][0],
											BaseString.Replace("CODE", options[i][1])
										)
							}
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
					maskManipulation masks = new maskManipulation();
					String msgText = "5 / ... \nЧто хочешь видеть на встречах клуба?";
					// the first one is the visible text, the second - its code
					String[][] options = new String[][] {
								new String [] {"Настольные игры🎲", "BOARDGAMES"},
								new String [] {"Видеоигры🎮", "VIDEOGAMES"},
								new String [] {"Книги📚", "BOOKS"},
								new String [] {"ИБДН📖", "BSFNR"},
								new String [] {"Еда🥗", "FOOD"},
								new String [] {"Структурированный материал🧑🏻‍🎓", "STRUCTUREDMATERIALS"},
								new String [] {"Свой вариант (ложит процесс)", "CUSTOM"}, // потенциальное решение: закэшировать весь код и отправить его в следующем сообщении бота, дождаться ответа юзера и после удалить оба сообщения, декэшировать и вернуть выбор
								new String [] {"Следующий вопрос", "NEXT"}
							};
					short numberOfOptions = 7, chosenMask = 0;
					if (bodyList.Count() == 6) { // The user is on this stage not for the first time
						chosenMask = short.Parse(bodyList[5]);
						if (chosenMask != 0) {
							numberOfOptions = 8; // now we include the 'next question' button
							msgText += "\nТвой выбор: ";
						}
						short counter = 0;
						for (short i = 0; i < 7; ++i)
							if (masks.isChosen(chosenMask, i)) { // the option is chosen
								++counter;
								msgText += ((counter != 1) ? ", " : "") + options[i][0];
							}
						Array.Resize<String>(ref bodyList, bodyList.Length - 1);
					}
					// it will have many interest options
					var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };
					Console.WriteLine("The mask is: " + chosenMask);
					for (short i = 0; i < numberOfOptions; ++i) {
						short tempMask = chosenMask;
						String resultString = "5";
						if (i == 7) // anything but the 'next' button doesn't change the stage
							resultString = "6";
						// now let's make a proper string: if the option was already chosen, it should be removed, if not, it should be added
						if (masks.isChosen(tempMask, i)) // the option is chosen: add the 'untick' text
							options[i][0] += " (убрать выбор)";
						tempMask = masks.flipChoise(tempMask, i);
						// merge the body into one string

						resultString +=
							commands.mainBodyOnly(bodyList) + ';'
							+ tempMask.ToString() + '|' + messageID;

						// Now we add each button to the inlineKeyboardList
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
				;
			case '6': { // now the user is choosing time
						// NOTE: make a module that operates just the same
						// as the code for the personal interests but has
						// time stamps, 'inconvenient' and 'the next day
						// of the week' buttons.

					String msgText = "6 / ... \nВ какие дни недели тебе бы хотелось посещать клуб?";

					var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };

					if (bodyList.Count() == 7) { // The user is on this stage not for the first time
						var chosenDaysStr = bodyList[6].Split(',');
						if (chosenDaysStr.Count() != 0) {
							//numberOfOptions = 8; // now we include the 'next question' button
							msgText += "\nТвой выбор: ";
						}
						//for (int i = 0; i < 7; ++i)

					}


					break;
				}
				;
			default: {
					commands.sendMsg(
						chatIdTemp,
						"Error: unknown command (it may be not available any more)"
					);
					break;
				}
				;
		}
	}

}
