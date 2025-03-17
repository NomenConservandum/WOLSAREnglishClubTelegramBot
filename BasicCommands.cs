using Telegram.Bot;

namespace BasicCommands {
    public class Commands {
        private ITelegramBotClient botClient;
        public Commands(ITelegramBotClient botClient) {
            this.botClient = botClient;
        }

        public void sendMsg(long chatID, String msg) {
            botClient.SendTextMessageAsync(
                chatID,
                msg
            );
            return;
        }
    }
}
