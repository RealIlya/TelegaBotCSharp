using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TelegaBot.Models;
using Telegram.Bot.Types;

namespace TelegaBot.BotStuff
{
    public interface ICommands
    {
        Task<Message> ShowHelp(Message message);
        Task<Message> ShowDuty(Message message);
        Task<Message> ShowTime(Message message);
        Task<Message> ShowTimetableSelector(Message message);
        Task ShowTimetable(CallbackQuery callbackQuery);
        Task<Message> ShowBlackFrame(Message message);
        Task<Message> ShowHomeWork(Message message);
        Task<Message> NewHomeWork(Message message);
        Task<Message> Echo(Message message, List<string> textList);
    }
}