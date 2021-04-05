using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;

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
            // Board = new Board();
            // Strategy = ...
        }

        public List<Player> Players => _players;
        public long ChatId => _chatId;
        public State State { get; set; }
        public bool IsStarted { get; set; } = false;
        public Board Board { get; }
        public Strategy Strategy { get; }
        
        
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
        
        

        // TODO send to chat method
    }


}