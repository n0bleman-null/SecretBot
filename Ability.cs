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
            await game.SendToChatAsync(@"Прэзідэнцкае права ""🔎 Даследаванне лаяльнасці"" даступна прэзідэнту");
            game.State = new RoleCheckAbilityState(game);
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList(),"Чыю ролю вы хочаце даведацца?");
        }
    }
    
    public class ShowLaws : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] ShowLaws executed");
            await game.SendToChatAsync($"Прэзідэнцкае права \"🔮 Прагляд законаў\" даступна прэзідэнту");
            await game.Board.President.SendMessageAsync($"Наступныя законы: {string.Join(" - ", game.Board.Deck.ShowLaws().Select(l => l switch {Law.Fascist => "фашысцкі", Law.Liberal => "ліберальны"}))}");
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
            await game.SendToChatAsync($"Прэзідэнцкае права \"👔 Пазачарговыя выбары\" даступна прэзідэнту");
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList(),"Каго абраць прэзідэнтам на пазачарговых выбарах?");
        }
    }
    
    public class Kill : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] Kill executed");
            game.State = new KillAbilityState(game);
            await game.SendToChatAsync($"Прэзідэнцкае права \"🗡 Знішчэнне\" даступна прэзідэнту");
            await game.SendChoiceAsync(game.Board.President, game.Players.Where(p => !p.IsAlive || p == game.Board.President).ToList(),"Каго забіць?");
        }
    }
    public class LiberalWin : IAbility
    {
        public async Task Execute(Game game)
        {
            Console.WriteLine($"[{DateTime.Now}] LiberalWins");  
            await game.SendToChatAsync($"Лібералы перамаглі");
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
            await game.SendToChatAsync($"Фашысты перамаглі");
            game.GameStatus = GameStatus.FascistWin;
            game.State = new EndGameState(game);
            await game.State.Step();
        }
    }
} 