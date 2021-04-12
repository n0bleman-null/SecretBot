using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public class Game
    {
        private List<Player> _players = new List<Player>();
        private long _chatId;

        public Game(long chat)
        {
            _chatId = chat;
            State = new PreparingState(this);
            //
            Strategy = new LowStrategy();
        }

        public List<Player> Players { get; set; }
        public long ChatId => _chatId;
        public State State { get; set; }
        public bool IsStarted { get; set; } = false;
        public Board Board { get; } = new Board();
        public Strategy Strategy { get; private set; }
        // for callbacks
        public long? CandidateForActionId { get; set; } = null;
        public Vote LastVoteResult { get; set; } = Vote.Undef;
        public List<Law> DraftedLaws { get; set; } = null;
        //
        
        public void Subscribe(User user) 
        {
            Players.Add(new Player(user));
        }
        public void Unsubscribe(User user)
        {
            Players.RemoveAll(player => player.User == user);
        }
        public bool IsSubscribed(User user)
        {
            return Players.Select(player => player.User).Contains(user);
        }
        public void Start()
        {
            // checks...
            State.Step();
        }

        public void CheckVotes()
        {
            if (Players.Any(player => player.VoteResult == Vote.Undef))
                return;
            if (Players.Count(player => player.VoteResult == Vote.Ya) > Players.Count / 2)
                LastVoteResult = Vote.Ya;
            else
                LastVoteResult = Vote.Nein;
            Players.ForEach(player => player.VoteResult = Vote.Undef);
            SendToChatAsync($"Результаты голосования {LastVoteResult}");
        }
        public async Task SendVoteAsync()
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Ya",$"{ChatId}:Vote:Ya"),
                    InlineKeyboardButton.WithCallbackData("Nein", $"{ChatId}:Vote:Nein")
                }
            );
            foreach (var player in Players.Where(player => player.IsAlive))
            {
                await Bot.Instance.SendTextMessageAsync(
                chatId: player.User.Id,
                text: "Голосуйте",
                replyMarkup: replyKeyboardMarkup);
            }
        }
        public async Task SendChoiceAsync(Player voting, List<Player> except = null)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(
                Players.Except(except ?? Enumerable.Empty<Player>()).Select(player
                    => InlineKeyboardButton.WithCallbackData(
                        string.Join(" ", player.User.FirstName, player.User.LastName),
                        $"{ChatId}:Choice:{player.User.Id}"))
            );
            await Bot.Instance.SendTextMessageAsync(
                chatId: voting.User.Id,
                text: "Выберите",
                replyMarkup: replyKeyboardMarkup);
        }

        public async Task SendPresidentDiscardLawAsync(List<Law> laws)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(laws.Select((law,index) => InlineKeyboardButton.WithCallbackData(
                law.ToString(),
                $"{ChatId}::{index}"
                )));
            await Bot.Instance.SendTextMessageAsync(
                chatId: Board.President.User.Id,
                text: "Выберите закон, который хотите скинуть",
                replyMarkup: replyKeyboardMarkup);
        }
        
        public async Task SendChancellorChooseLawAsync(List<Law> laws)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(laws.Select((law,index) => InlineKeyboardButton.WithCallbackData(
                law.ToString(),
                $"{ChatId}::{index}"
            )));
            await Bot.Instance.SendTextMessageAsync(
                chatId: Board.President.User.Id,
                text: "Выберите закон, который хотите принять",
                replyMarkup: replyKeyboardMarkup);
        }

        public async Task SendVetoRequestAsync(Player player)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Ya",$"{ChatId}:Vote:Ya"),
                    InlineKeyboardButton.WithCallbackData("Nein", $"{ChatId}:Vote:Nein")
                }
            );
            await Bot.Instance.SendTextMessageAsync(
                    chatId: player.User.Id,
                    text: "Воспользоваться правом вето?",
                    replyMarkup: replyKeyboardMarkup);
        }

        public async Task SendToChatAsync(string message)
        {
            await Bot.Instance.SendTextMessageAsync(
                chatId: ChatId,
                text: message);
        }
        
    }


}