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
            game.State = new PresidentElectionState(game);
            Console.WriteLine($"[{DateTime.Now}] Nothing happens");
            await game.State.Step();
        }
    }

    public class CheckRole : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] CheckRole executed");
            game.State = new RoleCheckAbilityState(game);
            await game.Board.President.SendMessageAsync("Чью роль пасмареть?");
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList());
        }
    }
    
    public class ShowLaws : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] ShowLaws executed");
            await game.Board.President.SendMessageAsync($"Next laws {string.Join(" ", game.Board.Deck.ShowLaws())}");
            game.State = new PresidentElectionState(game);
            await game.State.Step();
        }
    }
    
    public class EarlyElections : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] EarlyElections");
            game.EarlyElection = true;
            game.EarlyElectedPresident = game.Board.President;
            game.State = new EarlyElectionState(game);
            await game.Board.President.SendMessageAsync("Кого выбрать президентом?");
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList());
        }
    }
    
    public class Kill : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] Kill executed");
            game.State = new KillAbilityState(game);
            await game.Board.President.SendMessageAsync("Кого убить?");
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList());
        }
    }
    public class LiberalWin : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] LiberalWins");   
            game.GameStatus = GameStatus.LiberalWin;
            game.State = new EndGameState(game);
            await game.State.Step();
        }
    }
    
    public class FascistWin : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] FascistWin");   
            game.GameStatus = GameStatus.FascistWin;
            game.State = new EndGameState(game);
            await game.State.Step();
        }
    }
} 