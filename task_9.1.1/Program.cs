﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.ReplyMarkups;
using System.Linq;
using System.IO;
using Telegram.Bot.Types.InputFiles;
using System.Diagnostics;
using System.Text;

namespace task_9._1._1
{
    class Program
    {
        static string destinationFilePath;
        static string fileName;

        static void Main(string[] args)
        {
            string token = "5739146058:AAGQDZwvj2q1uw1aclAmC5iV0KZBlhyo5FE";

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
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;
                ReplyKeyboardMarkup keyboard = new(new[]
                {
                    new KeyboardButton[] { "It's wednesday, my dudes!" },
                    new KeyboardButton[] { "Среда, мои чуваки!" }
                })
                {
                    ResizeKeyboard = true,
                    OneTimeKeyboard = true
                };

                if (message.Type == MessageType.Text)
                {
                    if (message.Text.ToLower() == "/start")
                    {
                        await botClient.SendTextMessageAsync(message.Chat, "Добрый день. Просто пришли мне картинку документом" +
                                                                           " и посмотрим, что с ней можно сделать");

                        return;
                    }
                    else if (message.Text == "It's wednesday, my dudes!")
                    {
                        if (destinationFilePath != null)
                        {
                            Process.Start(@"D:\sb_homework\homework-9\WEDNESDAY.exe", $@"""{destinationFilePath}""");
                            await Task.Delay(3000);

                            await using Stream stream = System.IO.File.OpenRead(destinationFilePath);
                            await botClient.SendDocumentAsync(
                                chatId: message.Chat.Id,
                                document: new InputOnlineFile(stream, fileName.Replace("jpg", " (edited).jpg"))
                                );
                        }
                        return;
                    }
                    else if (message.Text == "Среда, мои чуваки!")
                    {
                        if (destinationFilePath != null)
                        {
                            Process.Start(@"D:\sb_homework\homework-9\СРЕДА.exe", $@"""{destinationFilePath}""");
                            await Task.Delay(3000);

                            await using Stream stream = System.IO.File.OpenRead(destinationFilePath);
                            await botClient.SendDocumentAsync(
                                chatId: message.Chat.Id,
                                document: new InputOnlineFile(stream, fileName.Replace("jpg", " (edited).jpg"))
                                );
                        }
                        return;
                    }
                }
                else if (message.Type == MessageType.Document)
                {
                    var fileId = update.Message.Document.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    fileName = CreateUniqueFileName() + ".jpg";
                    destinationFilePath = $@"D:\sb_homework\homework-9\user_photos\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    fileStream.Close();

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Выбери вариант обработки фотографии", replyMarkup: keyboard);

                    return;
                }
                else if (message.Type == MessageType.Photo)
                {
                    var fileId = update.Message.Photo.Last().FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    string fileName = CreateUniqueFileName() + ".jpg";

                    string destinationFilePath = $@"D:\sb_homework\homework-9\user_photos\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Фото скачано. " +
                                                         "Однако для обработки необходимо отправить его документом...");
                }
                else if (message.Type == MessageType.Audio)
                {
                    var fileId = update.Message.Audio.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    string fileName = CreateUniqueFileName() + ".mp3";

                    string destinationFilePath = $@"D:\sb_homework\homework-9\user_photos\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Аудио скачано. " +
                                                         "Однако для обработки необходимо отправить картинку документом...");
                }
                else if (message.Type == MessageType.Video)
                {
                    var fileId = update.Message.Video.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    string fileName = CreateUniqueFileName() + ".mp4";

                    string destinationFilePath = $@"D:\sb_homework\homework-9\user_photos\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Видео скачано. " +
                                                         "Однако для обработки необходимо отправить картинку документом...");
                }

                await botClient.SendTextMessageAsync(message.Chat, "Просто пришли мне картинку документом" +
                                                                   " и посмотрим, что с ней можно сделать");
            }
        }

        static string CreateUniqueFileName()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString().Remove(13)));
        }
}
}