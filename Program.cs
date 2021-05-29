using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;


namespace TelegramBot
{
    public enum CallbackType
    {
        Vote,
        SingleVote,
        Choice,
        DiscardLaw,
        ChooseLaw
    }
    public sealed class Bot 
    {
        private static readonly Lazy<TelegramBotClient> Lazy =
            new Lazy<TelegramBotClient>(() => new TelegramBotClient(token: Configuration.BotToken));
        public static TelegramBotClient Instance => Lazy.Value;
        private Bot()
        { }
    }
    
    public sealed class Games 
    {
        private static readonly Lazy<Dictionary<long, Game>> Lazy =
            new Lazy<Dictionary<long, Game>>(() => new Dictionary<long, Game>());
        public static Dictionary<long, Game> Instance => Lazy.Value;
        private Games()
        { }
    }
    
    class Program
    {
        static async Task Main(string[] args)
        {
            Bot.Instance.OnMessage += BotOnMessageReceived;
            Bot.Instance.OnCallbackQuery += BotOnCallbackQueryReceived;
            
            Bot.Instance.StartReceiving();
            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Bot.Instance.StopReceiving();
        }

        private static async void BotOnMessageReceived(object sender, MessageEventArgs messageEventArgs)
        {
            var message = messageEventArgs.Message;
            if (message == null || message.Type != MessageType.Text)
                return;

            if (message.Text.StartsWith('/'))
            {
                await Commands.ExecuteCommand(message);
            }
        }
        
        private static async void BotOnCallbackQueryReceived(object sender, CallbackQueryEventArgs callbackQueryEventArgs)
        {
            if (Games.Instance.Count == 0)
                return;
            var callbackQuery = callbackQueryEventArgs.CallbackQuery;
            var callback = callbackQuery.Data.Split(':');
            var chatId = long.Parse(callback[0]);
            var callbackType = (CallbackType) Enum.Parse(typeof(CallbackType), callback[1]);
            var callbackAnswer = callback[2];
            string data = default;
            switch (callbackType)
            {
                case CallbackType.Vote:
                    Games.Instance[chatId].Players.First(
                            player => player.User.Id == callbackQuery.From.Id).VoteResult =
                            (Vote) Enum.Parse(typeof(Vote), callbackAnswer);
                    data = callbackAnswer;
                    if (Games.Instance[chatId].AllVote())
                        await Games.Instance[chatId].State.Step();
                    break;
                case CallbackType.SingleVote:
                    Games.Instance[chatId].LastVoteResult = (Vote) Enum.Parse(typeof(Vote), callbackAnswer);
                    data = callbackAnswer;
                    await Games.Instance[chatId].State.Step();
                    break;
                case CallbackType.Choice:
                    Games.Instance[chatId].CandidateForActionId = long.Parse(callbackAnswer);
                    data = Games.Instance[chatId].Players.First(player => player.User.Id == long.Parse(callbackAnswer)).ToString();
                    await Games.Instance[chatId].State.Step();
                    break;
                case CallbackType.DiscardLaw:
                    data = Games.Instance[chatId].DraftedLaws.ElementAt(int.Parse(callbackAnswer)).ToString();
                    Games.Instance[chatId].DraftedLaws.RemoveAt(int.Parse(callbackAnswer));
                    await Games.Instance[chatId].State.Step();
                    break;
                case CallbackType.ChooseLaw:
                    data = Games.Instance[chatId].DraftedLaws.ElementAt((int.Parse(callbackAnswer) + 1) % 2).ToString();
                    Games.Instance[chatId].DraftedLaws.RemoveAt((int.Parse(callbackAnswer) + 1) % 2);
                    await Games.Instance[chatId].State.Step();
                    break;
            }
            await Bot.Instance.EditMessageTextAsync(
                callbackQuery.Message.Chat.Id,
                callbackQuery.Message.MessageId,
                $"Дзякуй, вы прагаласавалі за {data}",
                replyMarkup: null);
            // Console.WriteLine(callbackQuery.Data);
        }
    }
}