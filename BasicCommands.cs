using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;

namespace BasicCommands {
    public class Commands {
        private ITelegramBotClient botClient;
        public Commands() {}
        public Commands(ITelegramBotClient botClient) {
            this.botClient = botClient;
        }
		public void updateInlineMessage(long chatID, int messageID, String newText, InlineKeyboardMarkup newMarkup) {
			botClient.EditMessageText(chatID, messageID, newText, replyMarkup: newMarkup);
			return;
		}
		public void deleteMessage(long chatID, int messageID) {
			botClient.DeleteMessage(chatID, messageID);
			return;
		}
		public Message sendMsg(long chatID, String msg) {
            Task<Message> taskmsg = botClient.SendMessage(
                chatID,
                msg
            );
            return taskmsg.Result;
        }
        public Message sendMsgInline(long chatID, String msg, InlineKeyboardMarkup ikm) {
            Task<Message> taskmsg = botClient.SendMessage(
                    chatID,
                    msg,
                    replyMarkup: ikm
            );
			return taskmsg.Result;
        }
        // Either user clicks on the link or sends 'NEGATIVE' via callBackQuery
        public void sendInlineURL(long chatID, String msg, String msgLink, String msgNegative, String url) {
                var inlineKeyboard = new InlineKeyboardMarkup(
                    new List<InlineKeyboardButton[]>() {
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithUrl(msgLink, url),
                        },
                        new InlineKeyboardButton[] {
                            InlineKeyboardButton.WithCallbackData(msgNegative, "NEGATIVE"), 
                        },
                    }
                );
                sendMsgInline(
                    chatID,
                    msg,
                    inlineKeyboard
                );
               
                return;
        }
        public void showText(CallbackQuery callbackQuery, String msg) {
            botClient.AnswerCallbackQuery(callbackQuery.Id, msg);
            return;
        }
        public void showTextFullscreen(CallbackQuery callbackQuery, String msg) {
            botClient.AnswerCallbackQuery(callbackQuery.Id, msg, showAlert: true);
            return;
        }
        public String[] getDataFromUpdate(Update update) {
            String[] data = new String[3] {" ", " ", " "};
            String usernameTemp = "", msgTextTemp = "";
            long chatIdTemp = 0;
            
            switch (update.Type) { // to get the username, text of the message and the chat ID
                case UpdateType.Message: {
                    usernameTemp = update.Message.From.Username;
                    msgTextTemp = update.Message.Text;
                    chatIdTemp = update.Message.Chat.Id;
                    break;
                }
                case UpdateType.CallbackQuery: {
                    usernameTemp = update.CallbackQuery.From.Username;
                    msgTextTemp = update.CallbackQuery.Data;
                    chatIdTemp = update.CallbackQuery.Message.Chat.Id;
                    break;
                }
                default: {
                    usernameTemp = "NONE";
                    msgTextTemp = "";
                    chatIdTemp = 0;
                    break;
                    }
            }
            data[0] = usernameTemp;
            data[1] = msgTextTemp;
            data[2] = chatIdTemp.ToString();
            return data;
        }
    }
}
