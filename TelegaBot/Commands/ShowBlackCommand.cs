using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageProcessor.Plugins.WebP.Imaging.Formats;
using TelegaBot.BotStuff;
using TelegaBot.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;
using File = System.IO.File;

namespace TelegaBot.Commands
{
    public class ShowBlackCommand : ICommand
    {
        public async Task<Message> Execute(ITelegramBotClient client, Message message)
        {
            var wordsWithCommand = (message.Text ?? message.Caption)?.Split(' ').ToList();
            var wordsWithoutCommand = string.Join(" ", wordsWithCommand.RemoveACommand()).Split('\n').ToList();

            var photo = await client.GetFileAsync(((FileBase)message.Photo?.Last() ??
                                                   (FileBase)message.Document ?? (FileBase)message.Sticker ??
                                                   (FileBase)message.ReplyToMessage?.Photo?.Last() ??
                                                   (FileBase)message.ReplyToMessage?.Document ??
                                                   (FileBase)message.ReplyToMessage.Sticker).FileId);

            var ext = Path.GetExtension(photo.FilePath);
            
            var framePath = Path.Combine(GlConsts.ROOT_PATH, "frame.jpg");
            var photoPath = Path.Combine(GlConsts.ROOT_PATH, photo.FileUniqueId);
            var resultPath = Path.Combine(GlConsts.ROOT_PATH, "resultMeme.jpg");


            using (var fs = File.Create(photoPath))
            {
                await client.DownloadFileAsync(photo.FilePath, fs);
                fs.Close();
            }

            if (ext == ".webp")
            {
                var wpf = new WebPFormat();
                using var image = wpf.Load(File.Open(photoPath, FileMode.Open, FileAccess.ReadWrite));
                image.Save(photoPath);
                image.Dispose();
            }

            var phProcess = new PhotoProcessing();
            var result = phProcess.TextOnPhoto(phProcess.BlendingPhotos(framePath, photoPath),
                wordsWithoutCommand.First(), wordsWithoutCommand.Count > 1 ? wordsWithoutCommand.Last() : string.Empty);
            result.Save(resultPath, ImageFormat.Png);

            return await client.ToMessageAsync(message,
                new InputOnlineFile(new FileStream(resultPath, FileMode.Open)), null,
                ActionType.SendPhoto);
        }

        public bool CanExecute(Message message, out (string, AnnouncementType) error)
        {
            if (message is
            {
                Photo: null, Document: null, Sticker: null,
                ReplyToMessage: { Photo: null, Document: null, Sticker: null } or null
            })
            {
                error = (GlConsts.NOT_ENOUGH_ARGUMENTS + "/black <i>текст1\nтекст2 приложить картинку</i>",
                    AnnouncementType.Info);
                return false;
            }

            error = (null, AnnouncementType.Info);
            return true;
        }
    }
}