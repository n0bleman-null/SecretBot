using System;
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

        public List<Player> Players => _players;
        public long ChatId => _chatId;
        public State State { get; set; }
        public bool IsStarted { get; set; } = false;
        public Board Board { get; } = new Board();
        public Strategy Strategy { get; private set; }
        
        
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
        public async Task SendVote()
        {
            var replyKeyboardMarkup = new InlineKeyboardMarkup(new InlineKeyboardButton[]
                {
                    InlineKeyboardButton.WithCallbackData("Ya",$"{ChatId}:"),
                    InlineKeyboardButton.WithCallbackData("Nein")
                }
            );
            foreach (var player in Players)
            {
                await Bot.Instance.SendTextMessageAsync(
                chatId: player.User.Id,
                text: "Голосуйте",
                replyMarkup: replyKeyboardMarkup);
            }
        }
        

        // TODO send to chat method
    }


}