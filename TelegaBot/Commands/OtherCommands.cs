using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TelegaBot.BotStuff;
using TelegaBot.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace TelegaBot.Commands
{
    public class OtherCommands : GlobalConstants
    {
        private const string rootPath = @"C:\Users\Admin\Desktop\TelegaBot\TelegaBot\Img\";
        private static readonly Random _random = new Random();

        private static readonly List<Student> _students;

        private static int _lastSeenDay;

        private static List<Student> _todayDuties;


        static OtherCommands()
        {
            _lastSeenDay = DateTime.Today.Day;

            _students = File.Exists("Students.json")
                ? JsonConvert.DeserializeObject<List<Student>>(File.ReadAllText("Students.json"))
                : new List<Student>();
            _todayDuties = new List<Student>()
            {
                _students[_random.Next(0, _students.Count / 2)],
                _students[_random.Next(_students.Count / 2, _students.Count)]
            };
        }

        #region Commands block

        public static async Task<Message> ShowHelp(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message,
                "<b>Для просмотра дежурных</b> - /work\n" +
                "<b>Расписание</b> - /timetable\n" +
                "<b>Точное время</b> - /time\n" +
                "<b>Домашнее задание</b> - /hw\n" +
                "<b>Демотиватор</b> - /black <i>картинка</i>\n\n" +
                "<b>Параметры без запятых.</b>\n\n" +
                "<b>Сохранить домашнее задание в бота</b> -\n" +
                "/newhw <i>предмет, дата (или сегодня), день недели (или сегодня), приложить картинку</i>",
                ActionType.SendText);
        }

        public static async Task<Message> ShowDuty(ITelegramBotClient client, Message message)
        {
            var presentDay = DateTime.Today.Day;
            if (_lastSeenDay != presentDay)
            {
                _lastSeenDay = presentDay;

                _todayDuties = new List<Student>()
                {
                    _students[_random.Next(0, _students.Count / 2)],
                    _students[_random.Next(_students.Count / 2, _students.Count)]
                };
            }

            return await client.ToMessageAsync(message,
                $"Дежурят сегодня - <b>{string.Join(" и ", _todayDuties)}</b>",
                ActionType.SendText);
        }

        public static async Task<Message> ShowTime(ITelegramBotClient client, Message message)
        {
            return await client.ToMessageAsync(message,
                $"Дата: <b>{DateTime.Now.ToString($"d.M.yyyy")}</b>\n" +
                $"Время <b>{DateTime.Now.ToString($"H:m:s")}</b>",
                ActionType.SendText);
        }

        public static async Task<Message> Echo(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (textList.Count < 3)
                return await client.ToMessageAsync(message,
                    NotEnoughArguments +
                    "/echo <i>продолжительность задержка слово</i>",
                    ActionType.SendText, AnnouncementType.Info);

            var errorMessage = ((int.TryParse(textList.First(), out int checkDuration) && checkDuration <= 100),
                    (int.TryParse(textList[1], out int checkDelay) && checkDelay <= 10000),
                    (string.Join("", textList) is { Length: <= 100 })) switch
                {
                    (false, _, _) => client.ToMessageAsync(message,
                        "Продолжительность > 100 мс",
                        ActionType.SendText, AnnouncementType.Error),
                    (_, false, _) => client.ToMessageAsync(message,
                        "Задержка > 10000 мс",
                        ActionType.SendText, AnnouncementType.Error),
                    (_, _, false) => client.ToMessageAsync(message,
                        "Слово содержит > 100 букв",
                        ActionType.SendText, AnnouncementType.Error),
                    _ => null
                };

            if (errorMessage != null)
                return await errorMessage;

            var duration = int.Parse(textList.Pop(0));
            var delay = int.Parse(textList.Pop(0));

            for (int i = 0; i < duration; i++)
            {
                await Task.Delay(delay);
                await client.ToMessageAsync(message, string.Join(" ", textList),
                    ActionType.SendText);
            }

            return null;
        }

        private static Bitmap BlendingPhotos(string backgroundPath, string foregroundPath)
        {
            const int k = 71;
            const double d = 1.0855;

            var originalForeground = new Bitmap(foregroundPath);
            var background = new Bitmap(backgroundPath);
            var newForeground = new Bitmap(originalForeground,
                new Size((int)(background.Width / d) - k, (int)(background.Width / d) - 112 - k));


            for (int x = 0; x < newForeground.Width; x++)
            {
                for (int y = 0; y < newForeground.Height; y++)
                {
                    Color color = newForeground.GetPixel(x, y);
                    background.SetPixel(x + k, y + k, Color.FromArgb(color.R, color.G, color.B));
                }
            }

            originalForeground.Dispose();
            newForeground.Dispose();
            File.Delete(foregroundPath);

            return background;
        }

        private static Bitmap TextOnPhoto(Bitmap photo, string topText, string bottomText)
        {
            const int textHeight = 70;
            var topY = photo.Height / 2 + 330;
            var bottomY = topY + textHeight;

            var fromImage = Graphics.FromImage(photo);
            fromImage.DrawString(topText == string.Empty ? "Текст" : topText,
                new Font("Arial", 36, FontStyle.Italic), new SolidBrush(Color.White),
                new RectangleF(0, topY, photo.Width, textHeight),
                new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center });
            fromImage.DrawString(bottomText == string.Empty ? "Текст" : bottomText,
                new Font("Arial", 22, FontStyle.Regular), new SolidBrush(Color.White),
                new RectangleF(0, bottomY, photo.Width, textHeight),
                new StringFormat(StringFormatFlags.NoWrap) { Alignment = StringAlignment.Center });


            fromImage.Dispose();

            return photo;
        }

        public static async Task<Message> ShowBlack(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (message.Photo == null && message.Document == null && 
                message.ReplyToMessage.Photo == null && message.ReplyToMessage.Document == null)
                return await client.ToMessageAsync(message,
                    NotEnoughArguments +
                    "/black <i>текст1\nтекст2 приложить картинку</i>",
                    ActionType.SendText, AnnouncementType.Info);

            var newTextList = string.Join(" ", textList).Split('\n').ToList();

            var photo = await client.GetFileAsync(((FileBase)message.Photo?.Last() ?? message.Document ?? 
                (FileBase)message.ReplyToMessage.Photo?.Last() ?? message.ReplyToMessage.Document).FileId);

            var framePath = rootPath + "frame.jpg";
            var photoPath = rootPath + photo.FileUniqueId;
            var resultPath = rootPath + "resultMeme";

            using (var fs = new FileStream(photoPath, FileMode.Create))
            {
                await client.DownloadFileAsync(photo.FilePath, fs);
                fs.Close();
                fs.Dispose();
            }

            var result = TextOnPhoto(BlendingPhotos(framePath, photoPath), newTextList.First(),
                newTextList.Count > 1 ? newTextList.Last() : string.Empty);
            result.Save(resultPath, ImageFormat.Jpeg);


            return await client.SendPhotoAsync(message.Chat.Id,
                new InputOnlineFile(new FileStream(resultPath, FileMode.Open)),
                replyMarkup: MarkupConstructor.CreateMarkup());
        }

        public static async Task<Message> Insult(ITelegramBotClient client, Message message, List<string> textList)
        {
            if (message.ReplyToMessage == null)
                return await client.ToMessageAsync(message,
                    NotEnoughArguments +
                    "/insult <i>текст1\nтекст2 \"ответ на сообщение\"</i>",
                    ActionType.SendText, AnnouncementType.Info);
            
            var newTextList = string.Join(" ", textList).Split('\n').ToList();

            var userPhotoId = (await client.GetUserProfilePhotosAsync(message.ReplyToMessage.From.Id)).Photos.First()
                .Last().FileId;

            var userPhoto = await client.GetFileAsync(userPhotoId);
            var framePath = rootPath + "frame.jpg";
            var userPhotoPath = rootPath + userPhoto.FileUniqueId;
            var resultPath = rootPath + "resultInsult";

            using (var fs = new FileStream(userPhotoPath, FileMode.Create))
            {
                await client.DownloadFileAsync(userPhoto.FilePath, fs);
                fs.Close();
                fs.Dispose();
            }

            var result = TextOnPhoto(BlendingPhotos(framePath, userPhotoPath), newTextList.First(),
                newTextList.Count > 1 ? newTextList.Last() : string.Empty);
            result.Save(resultPath, ImageFormat.Jpeg);

            return await client.SendPhotoAsync(message.Chat.Id,
                new InputOnlineFile(new FileStream(resultPath, FileMode.Open)),
                replyMarkup: MarkupConstructor.CreateMarkup());
        }

        #endregion
    }
}