using System.Collections.ObjectModel;
using TelegaBot.Models;

namespace TelegaBot.BotStuff
{
    public class HwData
    {
        protected const string _haveNotHomework = "Домашнего задания ещё не добавлено!";
        protected static ObservableCollection<Homework> _homeworks;

        protected static Homework _notCheckedHomework;
    }
}