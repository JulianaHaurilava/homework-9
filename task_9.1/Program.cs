using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Extensions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types.ReplyMarkups;

namespace task_9._1
{
    class Program
    {
        static void Main(string[] args)
        {
            string token = "5549718456:AAGUEqjZL8OaXM0KM_O-CuDo8xmi-Ssp_nQ";

            var botClient = new TelegramBotClient($"{token}");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandleErrorAsync,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );

            Console.ReadKey();

            cts.Cancel();
        }

        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }

        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            //Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
            if (update.Type == UpdateType.Message && update?.Message?.Text != null)
            {
                var message = update.Message;
                ReplyKeyboardMarkup keybord = new(new[]
                {
                    new KeyboardButton[] {"Some button 1"}
                }
                    );

                if (message.Text.ToLower() == "/start")
                {
                    await botClient.SendTextMessageAsync(message.Chat, "Здесь нужен какой-то инфо-текст"); //Здесь нужен какой-то инфо-текст
                    
                    return;
                }
                await botClient.SendTextMessageAsync(message.Chat, "Дефолтный текст");//Дефолтный текст
            }
        }
    }
}
