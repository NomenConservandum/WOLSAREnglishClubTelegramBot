using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using DBEssentials;

public class firstEncounter {
	bool DEBUG = true;
	public firstEncounter () { }
	public void StartService (
				Update update,
				String usernameTemp, String msgTextTemp, long chatIdTemp,
				Commands commands, DBApi DB
			) {
		switch (update.Type) {
			case UpdateType.Message: { // greet the user and suggest them to go through a registration process
				int incomingMessageID = update?.Message?.Id ?? 1;
                Variables sensitive = new Variables();

                if (msgTextTemp != "/start") {
					commands.sendMsg(
						chatIdTemp,
						"Если что, команда: /start"
					);
					commands.deleteMessage(chatIdTemp, incomingMessageID);
					break;
				}

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

                var dataTransferAgreement = new InlineKeyboardMarkup(
					new List<InlineKeyboardButton[]>() {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithUrl("Согласие на обработку персональных данных", sensitive.getDTALink()),
						},
					}
				);
				var msg2 = commands.sendMsgInline(
					chatIdTemp,
					"Нажимая на 'Начать регистрацию!', Вы соглашаетесь на обработку персональных данных (текст согласия доступен ниже)",
					dataTransferAgreement
				);
				inlineKeyboard = new InlineKeyboardMarkup(
					new List<InlineKeyboardButton[]>() {
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithCallbackData(
								"Начать регистрацию!",
								"!REGISTRATION;" + msg1.Id.ToString() + ";" + msg2.Id.ToString()
							)
						},
						new InlineKeyboardButton[] {
							InlineKeyboardButton.WithUrl(
								"Узнать больше о клубе (FAQ)",
								sensitive.getFAQLink()
							)
						},
					}
				);

				// update the registration button to have ID's inside to pass them further
				commands.updateInlineMessage(
					chatIdTemp,
					msg1.Id,
					"Я хочу...",
					inlineKeyboard
				);

				break;
			};
			case UpdateType.CallbackQuery: {
                // we can safely split because a user cannot send a CallbackQuery message in this mode any other way
                var list = msgTextTemp.Split(';');
                switch (list[0]) {
					case "!REGISTRATION": {
                        if (DB.Add(new Users(chatIdTemp, usernameTemp, Statuses.AwaitingRegistrationChoice, Roles.NONE, 0))) // The user is added to the DB
                            return; // what? The user is already in the DB? Then they shouldn't be here!
                            
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
						commands.deleteMessage(chatIdTemp, int.Parse(list[2])); // delete the data transfer agreement message
							
							
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
}

