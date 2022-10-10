using System;
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
using System.Collections.Generic;

namespace task_9._1._1
{
    class Program
    {
        static string destinationFilePath;
        static string fileName;
        //static List<string> fileList = new List<string>();

        static void Main(string[] args)
        {
            string token = "5739146058:AAGQDZwvj2q1uw1aclAmC5iV0KZBlhyo5FE";

            var botClient = new TelegramBotClient($"{token}");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            string directoryName = $@"D:\sb_homework\homework-9\users_uploads";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

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

                string directoryName = $@"D:\sb_homework\homework-9\users_uploads\{message.From.Username}";

                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }

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

                    else if (System.IO.File.Exists($@"{directoryName}\{message.Text}"))
                    {
                        await using Stream stream = System.IO.File.OpenRead($@"{directoryName}\{message.Text}");

                        if (message.Text.EndsWith(".jpg"))
                        {
                            await botClient.SendPhotoAsync(message.Chat.Id, new InputOnlineFile(stream, fileName));
                        }
                        else if (message.Text.EndsWith(".mp4"))
                        {
                            await botClient.SendVideoAsync(message.Chat.Id, new InputOnlineFile(stream, fileName));
                        }
                        else if (message.Text.EndsWith(".mp3"))
                        {
                            await botClient.SendAudioAsync(message.Chat.Id, new InputOnlineFile(stream, fileName));
                        }
                        else
                        {
                            await botClient.SendAudioAsync(message.Chat.Id, new InputOnlineFile(stream, fileName));
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
                    destinationFilePath = $@"D:\sb_homework\homework-9\users_uploads\{message.From.Username}\{fileName}";
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

                    string destinationFilePath = $@"D:\sb_homework\homework-9\users_uploads\{message.From.Username}\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Фото скачано. " +
                                                         "Однако для обработки необходимо отправить его документом...");

                    return;
                }
                else if (message.Type == MessageType.Audio)
                {
                    var fileId = update.Message.Audio.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    string fileName = CreateUniqueFileName() + ".mp3";

                    string destinationFilePath = $@"D:\sb_homework\homework-9\users_uploads\{message.From.Username}\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Аудио скачано. " +
                                                         "Однако для обработки необходимо отправить картинку документом...");

                    return;
                }
                else if (message.Type == MessageType.Video)
                {
                    var fileId = update.Message.Video.FileId;
                    var fileInfo = await botClient.GetFileAsync(fileId);
                    var filePath = fileInfo.FilePath;
                    string fileName = CreateUniqueFileName() + ".mp4";

                    string destinationFilePath = $@"D:\sb_homework\homework-9\users_uploads\{message.From.Username}\{fileName}";
                    await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
                    await botClient.DownloadFileAsync(filePath, fileStream);

                    await botClient.SendTextMessageAsync(message.Chat.Id, "Видео скачано. " +
                                                         "Однако для обработки необходимо отправить картинку документом...");

                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat, "Список всех загруженных файлов:\n" +
                                                                    GetFilesNameString(directoryName));
                await botClient.SendTextMessageAsync(message.Chat, "Для того, чтобы скачать любой файл, напиши \"название_файла\"");
            }
        }

        static string GetFilesNameString(string directoryName)
        {
            string fileNameString = "";
            DirectoryInfo directory = new DirectoryInfo(directoryName); // папка с файлами 

            foreach (FileInfo file in directory.GetFiles())
            {
                fileNameString += Path.GetFileName(file.FullName) + "\n";
            }

            return fileNameString;
        }

        static string CreateUniqueFileName()
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString().Remove(13))).TrimEnd('=');
        }
}
}
