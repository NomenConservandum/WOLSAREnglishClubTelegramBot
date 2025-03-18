using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;
using Telegram.Bot.Exceptions;

namespace BasicCommands {
    public class Commands {
        private ITelegramBotClient botClient;
        public Commands(ITelegramBotClient botClient) {
            this.botClient = botClient;
        }

        public void sendMsg(long chatID, String msg) {
            botClient.SendMessage(
                chatID,
                msg
            );
            return;
        }
        public void sendMsgInline(long chatID, String msg, InlineKeyboardMarkup ikm) {
            botClient.SendMessage(
                    chatID,
                    msg,
                    replyMarkup: ikm
                    );
            return;
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
    }
}
