/// <summary>
/// WOLSAR English club Telegram Bot
/// The telegram bot for an English
/// club on the basis of LROoCoEF WOLSAR
/// </summary>


class Program {
    static async Task Main() {
        Bot bot = new Bot();
        var me = bot.getMe();
        Console.WriteLine($"The ID of the bot is {me?.Id ?? 0}\nThe name of the bot is {me?.FirstName ?? ""}.");
        await Task.Delay(-1);
    }
}
