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

namespace task_9._1._1
{
    class Program
    {
        //я понимаю, что нужно сделать эти переменные локальными, но я не понимаю, как их передать в метод HandleUpdateAsync
        static string filePathToModify; //путь к файлу, который необходимо отредактировать
        static string fileNameToModify; //имя файла, который нужно отредактировать
        static string finalPath; //Путь к программе
        //

        static void Main(string[] args)
        {
            string token = "";

            var botClient = new TelegramBotClient($"{token}");

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>()
            };

            SetFinalPath();

            string directoryName = $@"{finalPath}\users_uploads";

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,
                pollingErrorHandler: HandleError,
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
                );

            Console.ReadKey();

            cts.Cancel();
        }

        /// <summary>
        /// Выводит информацию об ошибке в консоль
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="exception"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Task HandleError(ITelegramBotClient botClient, Exception exception,
                                       CancellationToken cancellationToken)
        {
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обрабатывает сообщения пользователя
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        async static Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken token)
        {
            if (update.Type == UpdateType.Message)
            {
                var message = update.Message;

                string directoryName = $@"{finalPath}\users_uploads\{message.From.Username}";

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

                MessageType typeOfMessage;
                typeOfMessage = message.Type;

                switch (typeOfMessage)
                {
                    case MessageType.Text:
                        {
                            string messageText = message.Text;
                            switch (messageText)
                            {
                                case "/start":
                                    {
                                        await botClient.SendTextMessageAsync(message.Chat, "Этот бот может создавать поздравления" +
                                                                           " с великим праздником Жабья Среда!");
                                        await botClient.SendTextMessageAsync(message.Chat, "Просто пришли картинку документом" +
                                                                           " и посмотрим, что с ней можно сделать");
                                        return;
                                    }
                                case "It's wednesday, my dudes!":
                                    {
                                        string editorPath = $@"{finalPath}\WEDNESDAY.exe";

                                        if (filePathToModify != null)
                                        {
                                            ModifyDocument(editorPath);
                                            SendModyfiedDocumentAsync(botClient, message);
                                        }

                                        return;
                                    }
                                case "Среда, мои чуваки!":
                                    {
                                        string editorPath = $@"{finalPath}\СРЕДА.exe";

                                        if (filePathToModify != null)
                                        {
                                            ModifyDocument(editorPath);
                                            SendModyfiedDocumentAsync(botClient, message);
                                        }

                                        return;
                                    }                                   
                            }
                            break;
                        }
                    case MessageType.Document:
                        {
                            DownloadDocumentAsync(botClient, update, message);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Выбери вариант обработки фотографии", replyMarkup: keyboard);

                            return;
                        }
                    case MessageType.Photo:
                        {
                            var fileId = update.Message.Photo.Last().FileId;
                            DownloadFileAsync(botClient, update, message, typeOfMessage, fileId);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Фото скачано. " +
                                                         "Однако для обработки необходимо отправить его документом...");

                            return;
                        }
                    case MessageType.Video:
                        {
                            var fileId = update.Message.Video.FileId;
                            DownloadFileAsync(botClient, update, message, typeOfMessage, fileId);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Видео скачано. " +
                                                         "Однако для обработки необходимо отправить картинку документом...");

                            return;
                        }
                    case MessageType.Audio:
                        {
                            var fileId = update.Message.Audio.FileId;
                            DownloadFileAsync(botClient, update, message, typeOfMessage, fileId);
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Аудио скачано. " +
                                                         "Однако для обработки необходимо отправить картинку документом...");

                            return;
                        }
                }

                if (System.IO.File.Exists($@"{directoryName}\{message.Text}"))
                {
                    await using Stream stream = System.IO.File.OpenRead($@"{directoryName}\{message.Text}");

                    if (message.Text.EndsWith(".jpg"))
                    {
                        await botClient.SendPhotoAsync(message.Chat.Id, new InputOnlineFile(stream, fileNameToModify));
                    }
                    else if (message.Text.EndsWith(".mp4"))
                    {
                        await botClient.SendVideoAsync(message.Chat.Id, new InputOnlineFile(stream, fileNameToModify));
                    }
                    else if (message.Text.EndsWith(".mp3"))
                    {
                        await botClient.SendAudioAsync(message.Chat.Id, new InputOnlineFile(stream, fileNameToModify));
                    }
                    else
                    {
                        await botClient.SendAudioAsync(message.Chat.Id, new InputOnlineFile(stream, fileNameToModify));
                    }
                    return;
                }

                await botClient.SendTextMessageAsync(message.Chat, "Список всех загруженных файлов:\n" +
                                                                    GetFilesNameString(directoryName));
                await botClient.SendTextMessageAsync(message.Chat, "Для того, чтобы скачать любой файл, напиши \"название_файла\"");
            }
        }

        /// <summary>
        /// Скачивает документ, который прислал пользователь
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="message"></param>
        async static void DownloadDocumentAsync(ITelegramBotClient botClient, Update update, Message message)
        {
            var fileId = update.Message.Document.FileId;
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;
            fileNameToModify = CreateUniqueFileName(MessageType.Document);
            filePathToModify = $@"{finalPath}\users_uploads\{message.From.Username}\{fileNameToModify}";
            await using FileStream fileStream = System.IO.File.OpenWrite(filePathToModify);
            await botClient.DownloadFileAsync(filePath, fileStream);

            fileStream.Close();
        }

        /// <summary>
        /// Скачивает любой файл, который присла пользователь, кроме документа
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="update"></param>
        /// <param name="message"></param>
        /// <param name="typeOfMessage"></param>
        /// <param name="fileId"></param>
        async static void DownloadFileAsync(ITelegramBotClient botClient, Update update, Message message, 
                                            MessageType typeOfMessage, string fileId)
        {
            var fileInfo = await botClient.GetFileAsync(fileId);
            var filePath = fileInfo.FilePath;
            string fileName = CreateUniqueFileName(typeOfMessage);
            string destinationFilePath = $@"{finalPath}\users_uploads\{message.From.Username}\{fileName}";
            await using FileStream fileStream = System.IO.File.OpenWrite(destinationFilePath);
            await botClient.DownloadFileAsync(filePath, fileStream);

            fileStream.Close();
        }
        /// <summary>
        /// Возвращает строку из имен файлов, которые прислал пользователь
        /// </summary>
        /// <param name="directoryName"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Создает уникальное имя файлу
        /// </summary>
        /// <param name="typeOfMessage"></param>
        /// <returns></returns>
        static string CreateUniqueFileName(MessageType typeOfMessage)
        {
            var fileNameString = Convert.ToBase64String(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString().Remove(13))).TrimEnd('=');
            switch (typeOfMessage)
            {
                case MessageType.Audio:
                    {
                        fileNameString += ".mp3";
                        break;
                    }
                case MessageType.Video:
                    {
                        fileNameString += ".mp4";
                        break;
                    }
                case MessageType.Document:
                case MessageType.Photo:
                    {
                        fileNameString += ".jpg";
                        break;
                    }
            }
            return fileNameString;

        }

        /// <summary>
        /// Редактирует документ, который прислал пользоваетель
        /// </summary>
        /// <param name="editorPath"></param>
        static void ModifyDocument(string editorPath)
        {
            Process.Start(editorPath, $@"""{filePathToModify}""");
        }

        /// <summary>
        /// Отправляет отредактированные документы
        /// </summary>
        /// <param name="botClient"></param>
        /// <param name="message"></param>
        async static void SendModyfiedDocumentAsync(ITelegramBotClient botClient, Message message)
        {
            await Task.Delay(3000);
            await using Stream stream = System.IO.File.OpenRead(filePathToModify);
            await botClient.SendDocumentAsync(message.Chat.Id, new InputOnlineFile(stream, fileNameToModify.Replace("jpg", " (edited).jpg")));
            stream.Close();
        }

        /// <summary>
        /// Определяет путь к программе
        /// </summary>
        static void SetFinalPath()
        {
            string path = Directory.GetCurrentDirectory();
            int indexToRemove = path.IndexOf("task_9.1.1") + 10;
            finalPath = path.Remove(indexToRemove);
        }
    }
}
