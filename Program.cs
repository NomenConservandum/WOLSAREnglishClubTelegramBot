// WOLSAR English club Telegram Bot source code

using BotAPI;

class Program {
    static async Task Main() {
        Bot bot = new Bot();
        var me = await bot.getMe();
        Console.WriteLine($"The ID of the bot is {me.Id}\nThe name of the bot is {me.FirstName}.");
        await Task.Delay(-1);
    }
}
