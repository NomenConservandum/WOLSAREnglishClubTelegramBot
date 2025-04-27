using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using DBEssentials;

public class RegistrationProcessService {
	bool DEBUG = true;
	public RegistrationProcessService () { }
	public void StartServiceCustomer (
			Update update,
			String usernameTemp, String msgTextTemp, long chatIdTemp,
			Commands commands, DBApi RegistrationFormsDB, DBApi UsersDB

        ) {

		/*
		if (list[0] == "!UNDO") {
            BaseDBModel elementTemp = RegistrationFormsDB.findByField(FieldsDB.Username, '\'' + usernameTemp + '\'');
            FillOutFormParticipants formTemp = elementTemp as FillOutFormParticipants ?? new FillOutFormParticipants();
            formTemp.stage -= 2;
            if (RegistrationFormsDB.UpdateByField(formTemp, FieldsDB.Username, '\'' + usernameTemp + '\''))
                return;
        }
		*/
		// get the form out of the DB
		BaseDBModel element = RegistrationFormsDB.findByField(FieldsDB.Username, '\'' + usernameTemp + '\'');
		FillOutFormParticipants form = element as FillOutFormParticipants ?? new FillOutFormParticipants();


        String[]? list = ["NOTHING", "0"];

        String messageID = "0";

        if (!form.isValid()) {
            commands.sendMsg(
                chatIdTemp,
                """
                An error has occured
                """
            );
			return;
        }
        if (msgTextTemp.IndexOf('|') == -1 && form.stage != RegistrationStatuses.AwaitingConductorResponse) {
            commands.sendMsg(
                chatIdTemp,
                """
                Error: unknown command (it may be not available any more)
                Type /rescue for further help
                """
            );
            int incomingMessageID = update?.Message?.Id ?? 1;
            commands.deleteMessage(chatIdTemp, incomingMessageID);
            return;
        } else if (msgTextTemp.IndexOf('|') != -1 ) {
			list = msgTextTemp.Split('|');
			messageID = list[1];
        }
		switch (form.stage) {
			case RegistrationStatuses.AwaitingRoleChoice: {
                form.stage = RegistrationStatuses.AwaitingSexChoice;
                if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
                    return;
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>() {
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData("Мужской", "MALE|" + messageID),
                            InlineKeyboardButton.WithCallbackData("Женский", "FEMALE|" + messageID),
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
			}
			case RegistrationStatuses.AwaitingSexChoice: { // for some reason it only goes here
				if (DEBUG) Console.WriteLine($"User {usernameTemp} is {form.sex.ToString()}");
				form.sex = (list[0] == "MALE" ? Genders.Male : Genders.Female);
				form.stage = RegistrationStatuses.AwaitingAgeChoice;
                if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
					return;

                var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };
				for (short i = 15; i < 26; i += 4) {
					inlineKeyboardList.Add(
						[
							InlineKeyboardButton.WithCallbackData(i.ToString(), i.ToString() + '|' + messageID),
							InlineKeyboardButton.WithCallbackData((i + 1).ToString(), (i + 1).ToString() + '|' + messageID),
							InlineKeyboardButton.WithCallbackData((i + 2).ToString(), (i + 2).ToString() + '|' + messageID),
							InlineKeyboardButton.WithCallbackData((i + 3).ToString(), (i + 3).ToString() + '|' + messageID)
						]
					);
				}
                inlineKeyboardList.Add(
                    [InlineKeyboardButton.WithCallbackData("Предыдущий вопрос", "!UNDO|" + messageID)]
                );
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
			case RegistrationStatuses.AwaitingAgeChoice: {
				form.age = Int32.Parse(list[0]);
				form.stage = RegistrationStatuses.AwaitingLevelChoice;
                if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
                    return;
                if (DEBUG) Console.WriteLine($"User {usernameTemp} is {form.age} years old");
				// sends commands that will be available only on the next stage
				String[] levelsDescription =
                    [
						"Нулевой: Знаю пару слов максимум😜",
						"A1: Могу делать простые предложения🎩",
						"A2: Могу рассказать, что делаю по жизни",
						"B1: Могу легко делиться мнением / хобби",
						"B2: Могу легко общаться на разные темы",
						"C1: Могу изъясняться легко и спонтанно💅"
					];
				var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };
				for (int i = 0; i < 6; ++i)
					inlineKeyboardList.Add(
						[InlineKeyboardButton.WithCallbackData(levelsDescription[i] , i.ToString() + '|' + messageID)]
					);

                inlineKeyboardList.Add(
                    [InlineKeyboardButton.WithCallbackData("Предыдущий вопрос", "!UNDO|" + messageID)]
                );

                var inlineKeyboard = new InlineKeyboardMarkup(inlineKeyboardList);
				commands.updateInlineMessage(
					chatIdTemp,
					int.Parse(messageID),
					"3 / ... \nТвой уровень владения языком:",
					inlineKeyboard
				);
				break;
			};
			case RegistrationStatuses.AwaitingLevelChoice: {
                form.languageProficiency = (proficiencyLevels)Int32.Parse(list[0]);
                form.stage = RegistrationStatuses.AwaitingFormatChoice;
                if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
                    return;
                Console.WriteLine($"User {usernameTemp} has {form.languageProficiency} level knowledge about English.");
                
				//tempBody = '4' + commands.mainBodyOnly(bodyList); // guarantees the next stage. Now this variable is used as a temporary string.
				var inlineKeyboard = new InlineKeyboardMarkup(
						new List<InlineKeyboardButton[]>() {
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("Оффлайн", "OFFLINE|" + messageID)
							},
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("Онлайн", "ONLINE|" + messageID)
							},
							new InlineKeyboardButton[] {
								InlineKeyboardButton.WithCallbackData("Предыдущий вопрос", "!UNDO|" + messageID)
							}
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
			case RegistrationStatuses.AwaitingFormatChoice: {
				form.format = (list[0] == "OFFLINE" ? Formats.Offline : Formats.Online);
                form.stage = RegistrationStatuses.AwaitingConductorChoice;
                if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
                    return;

                //tempBody = commands.mainBodyOnly(bodyList); // Does NOT guarantee the next stage. Now this variable is used as a temporary string.
				Console.WriteLine($"User {usernameTemp} has chosen {list[0]} meetings.");
				// the first one is the visible text, the second - its code
				String[][] options = [
						["", ""],
                        ["", ""],
                        ["", ""],
						["Свой вариант", list[0] + ":OTHER|" + messageID],
					];
				String msgText = "";
				switch (list[0]) {
					case "OFFLINE": {
						msgText = "4 / ... \nТвой выбор: оффлайн встречи.\nГде ты предпочтёшь встречаться?";
						options[0] = ["В здании церкви", "CHURCH"]; // TODO: Добавить адрес при получении разрешения
						options[1] = ["Тайм-кафе (дружба, лофт и другие)", "TIME-CAFE"];
						options[2] = ["TEMPORARY UNAVAILABLE", "NONE"]; // FIX: Заменить либо на СГУ, либо на что-то другое нейтральное
						break;
					}
					case "ONLINE": {
						msgText = "4 / ... \nТвой выбор: онлайн встречи.\nГде ты предпочтёшь созваниваться?";
						options[0] = ["в Telegram", "TELEGRAM"];
						options[1] = ["в Discord", "DISCORD"];
						options[2] = ["в VK", "VK"];
						break;
					}
				}

				var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };

				for (int i = 0; i < 4; ++i)
					inlineKeyboardList.Add(
						[InlineKeyboardButton.WithCallbackData(options[i][0], options[i][1] + '|' + messageID)]
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
			case RegistrationStatuses.AwaitingConductorResponse: {
				form.stage = RegistrationStatuses.AwaitingConductorChoice;
				form.conductor = msgTextTemp;
				if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
					return;
				commands.deleteMessage(chatIdTemp, update?.Message?.Id ?? 1);
				break;
			}
            case RegistrationStatuses.AwaitingConductorChoice: {
				if (list[0].IndexOf(':') != -1) { // the user has chosen 'other' option

					var msg = commands.sendMsg(
						chatIdTemp,
						"Напиши свой вариант (Формат сообщения: не более 20 символов)"
					);
                    commands.updateInlineMessage(
                        chatIdTemp,
                        int.Parse(messageID),
                        "Продолжить?",
                        new InlineKeyboardMarkup(new List<InlineKeyboardButton[]>() { new InlineKeyboardButton[] { InlineKeyboardButton.WithCallbackData("Да!", msg.Id.ToString() + '|' + messageID) } })
                    );
                    form.stage = RegistrationStatuses.AwaitingConductorResponse;
					if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
						return;
					break;
				}
				commands.deleteMessage(chatIdTemp, Int32.Parse(list[0]));
				// TODO: make it a proper mode. The command above will also try to delete the message with ID equal to mask, which is cringe
				form.stage = RegistrationStatuses.InterestsChoice;
				if (RegistrationFormsDB.UpdateByField(form, FieldsDB.Username, '\'' + usernameTemp + '\''))
						return;

				maskManipulation masks = new maskManipulation();
				String msgText = "5 / ... \nЧто хочешь видеть на встречах клуба?";
				// the first one is the visible text, the second - its code
				String[][] options = new String[][] {
					["Настольные игры🎲", "BOARDGAMES"],
					["Видеоигры🎮", "VIDEOGAMES"],
					["Книги📚", "BOOKS"],
					["ИБДН📖", "BSFNR"],
					["Еда🥗", "FOOD"],
					["Структурированный материал🧑🏻‍🎓", "STRUCTUREDMATERIALS"],
					["Свой вариант", "OTHER"], // потенциальное решение: закэшировать весь код и отправить его в следующем сообщении бота, дождаться ответа юзера и после удалить оба сообщения, декэшировать и вернуть выбор
					["Следующий вопрос", "NEXT"]
				};
				short numberOfOptions = 7, chosenMask = (short)form.interestsMask;
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
				
				var inlineKeyboardList = new List<InlineKeyboardButton[]>() { };
				Console.WriteLine("The mask is: " + chosenMask);
				for (short i = 0; i < numberOfOptions; ++i) {
					short tempMask = chosenMask;
					String resultString = "";
					// now let's make a proper string: if the option was already chosen, it should be removed, if not, it should be added
					if (masks.isChosen(tempMask, i)) // the option is chosen: add the 'untick' text
						options[i][0] += " (убрать выбор)";
					tempMask = masks.flipChoise(tempMask, i);
					// merge the body into one string

					resultString = tempMask.ToString() + '|' + messageID;

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
			case RegistrationStatuses.InterestsChoice: {
				break;
			}
			case RegistrationStatuses.AwaitingInterestsChoice: { // now the user is choosing time
					// NOTE: make a module that operates just the same
					// as the code for the personal interests but has
					// time stamps, 'inconvenient' and 'the next day
					// of the week' buttons.
					/*
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
					*/

				break;
			}
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
