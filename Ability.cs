using System;
using System.Linq;
using System.Threading.Tasks;

namespace TelegramBot
{
    public interface IAbility
    {
        public Task Execute(Game game);
    }

    public class Nothing : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] Nothing happens");
            await game.State.Step();
        }
    }

    public class CheckRole : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] CheckRole executed");
            game.State = null;
            await game.Board.President.SendMessageAsync("Чью роль пасмареть?");
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList());
            
        }
    }
} 