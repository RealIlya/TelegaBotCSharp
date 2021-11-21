using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace TelegaBot.Commands
{
    public class InsultCommand : ICommand
    {
        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            var wordsWithoutCommand =
                string.Join(" ", (message.Text ?? message.Caption)?.Split(' ').ToList().RemoveACommand()!)
                    .Split('\n').ToList();
            
            string userPhotoId = (await client.GetUserProfilePhotosAsync(message.ReplyToMessage.From.Id)).Photos.First()
                .Last().FileId;

            var userPhoto = await client.GetFileAsync(userPhotoId);
            var framePath = GlConsts.ROOT_PATH + "frame.jpg";
            var userPhotoPath = GlConsts.ROOT_PATH + userPhoto.FileUniqueId;
            var resultPath = GlConsts.ROOT_PATH + "resultInsult";

            using (var fs = new FileStream(userPhotoPath, FileMode.Create))
            {
                await client.DownloadFileAsync(userPhoto.FilePath, fs);
                fs.Close();
                fs.Dispose();
            }

            var phProccess = new PhotoProcessing();
            var result = phProccess.TextOnPhoto(phProccess.BlendingPhotos(framePath, userPhotoPath),
                wordsWithoutCommand.First(), wordsWithoutCommand.Count > 1 ? wordsWithoutCommand.Last() : string.Empty);
            result.Save(resultPath, ImageFormat.Jpeg);

            return await client.ToMessageAsync(message,
                new InputOnlineFile(new FileStream(resultPath, FileMode.Open)), null,
                ActionType.SendPhoto);
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            if (message.ReplyToMessage == null)
            {
                error = (GlConsts.NOT_ENOUGH_ARGUMENTS + "/insult <i>текст1\nтекст2 \"ответ на сообщение\"</i>",
                    AnnouncementType.Info);
                return false;
            }

            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}