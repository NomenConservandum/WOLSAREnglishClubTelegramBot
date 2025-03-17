// WOLSAR English club Telegram Bot source code

using Telegram.Bot;
using Sensitive;
Variables sensitive = new Variables();

var bot = new TelegramBotClient(sensitive.getToken());
var me = await bot.GetMe();
Console.WriteLine($"The ID of the bot is {me.Id}\nThe name of the bot is {me.FirstName}.");
