using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot
{
    public enum GameStatus
    {
        Preparing,
        Started,
        FascistWin,
        LiberalWin
    }
    public class Game
    {
        private long _chatId; // TODO make auto property

        public Game(long chat)
        {
            _chatId = chat;
            State = new PreparingState(this);
        }

        public List<Player> Players { get; set; } = new List<Player>();
        public long ChatId => _chatId;
        public State State { get; set; }
        public bool IsStarted => GameStatus != GameStatus.Preparing;
        public GameStatus GameStatus { get; set; }= GameStatus.Preparing;
        
        public Board Board { get; } = new Board();
        public Strategy Strategy { get; set; }
        // for callbacks
        public long? CandidateForActionId { get; set; } = null;
        public Vote LastVoteResult { get; set; } = Vote.Undef;
        public List<Law> DraftedLaws { get; set; } = null;
        // for early election
        public bool EarlyElection { get; set; } = false;
        public Player EarlyElectedPresident { get; set; } = null;
        
        
        
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

        public bool AllVote()
        {
            var alive = Players.Where(player => player.IsAlive);
            if (alive.Any(player => player.VoteResult == Vote.Undef))
                return false;
            LastVoteResult = alive.Count(player => player.VoteResult == Vote.Ya) > Players.Count / 2
                ? Vote.Ya
                : Vote.Nein;
            Players.ForEach(player => player.VoteResult = Vote.Undef);
            return true;
        }
        public async Task SendVoteAsync(string txt)
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
                text: txt,
                replyMarkup: replyKeyboardMarkup);
            }
        }
        public async Task SendChoiceAsync(Player voting, List<Player> except = null, string txt = "Выберите")
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(
                Players.Except(except ?? Enumerable.Empty<Player>()).Select(player
                    => InlineKeyboardButton.WithCallbackData(
                        string.Join(" ", player.User.FirstName, player.User.LastName),
                        $"{ChatId}:Choice:{player.User.Id}"))
            );
            var mes = await Bot.Instance.SendTextMessageAsync(
                chatId: voting.User.Id,
                text: txt,
                replyMarkup: replyKeyboardMarkup);
            
        }
        public async Task SendPresidentDiscardLawAsync(string txt)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(DraftedLaws.Select((law,index) => InlineKeyboardButton.WithCallbackData(
                law.ToString(),
                $"{ChatId}:DiscardLaw:{index}"
                )));
            await Bot.Instance.SendTextMessageAsync(
                chatId: Board.President.User.Id,
                text: txt,
                replyMarkup: replyKeyboardMarkup);
        }
        public async Task SendChancellorChooseLawAsync(string txt)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(DraftedLaws.Select((law,index) => InlineKeyboardButton.WithCallbackData(
                law.ToString(),
                $"{ChatId}:ChooseLaw:{index}"
            )));
            await Bot.Instance.SendTextMessageAsync(
                chatId: Board.Chancellor.User.Id,
                text: txt,
                replyMarkup: replyKeyboardMarkup);
        }

        public async Task SendVetoRequestAsync(Player player)
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Ya",$"{ChatId}:SingleVote:Ya"),
                    InlineKeyboardButton.WithCallbackData("Nein", $"{ChatId}:SingleVote:Nein")
                }
            );
            await Bot.Instance.SendTextMessageAsync(
                    chatId: player.User.Id,
                    text: "Хотите воспользоваться правом вето?",
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