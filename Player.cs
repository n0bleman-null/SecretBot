using Telegram.Bot.Types;

namespace TelegramBot
{
    public enum Role
    {
        Liberal,
        Fascist
    }

    public enum Person
    {
        Liberal,
        Fascist,
        Hitler
    }

    public class Player
    {
        public Player(User user)
        {
            User = user;
        }
        
        public User User { get; set; }
        public bool IsDead { get; set; } = false;
        public bool MaybeHitler { get; set; } = true;
        public Role Role { get; set; }
        public Person Person { get; set; }
        
        // TODO send private message method
    }
}