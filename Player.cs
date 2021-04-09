using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

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

    public enum Vote
    {
        Ya,
        Nein,
        Undef
    }

    public class Player
    {
        public Player(User user)
        {
            User = user;
        }
        public User User { get; set; }
        public bool IsAlive { get; set; } = true;
        public bool MaybeHitler { get; set; } = true;
        public Role Role { get; set; }
        public Person Person { get; set; }
        public Vote VoteResult { get; set; } = Vote.Undef;

        public async Task SendMessageAsync(string message)
        {
            await Bot.Instance.SendTextMessageAsync(
                    chatId: User.Id,
                    text: message);
        }
        
    }
}