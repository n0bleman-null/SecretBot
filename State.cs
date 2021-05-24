using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot.Types.ReplyMarkups;
namespace TelegramBot
{
    
    public abstract class State // TODO state machine facade is ready, after realizing Game.Board make implementations 
    {
        protected Game Game;
        protected State(Game game)
        {
            Game = game;
        }
        public abstract Task Step();
    }

    class PreparingState : State
    {
        public PreparingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            foreach (var (player,person) in Enumerable.Zip(Game.Players, Game.Strategy.GetRoles(Game.Players.Count)))
            {
                player.Person = person;
                player.Role = person switch
                {
                    Person.Fascist => Role.Fascist,
                    Person.Hitler => Role.Fascist,
                    Person.Liberal => Role.Liberal
                };
            }

            Game.Players = Game.Players.OrderBy(a => Strategy.Randomizer.Next()).ToList();
            Game.Board.President = Game.Players.Last();
            Console.WriteLine($"[{DateTime.Now}] Game started");
            Game.Players.ForEach(player => player.SendMessageAsync($"{player.Role}\n{player.Person}"));
            Game.State = new PresidentElectionState(Game);
            Game.State.Step();
        }
    }
    
    class PresidentElectionState : State
    {
        public PresidentElectionState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Game.Board.LastPresident = Game.Board.President;
            Game.Board.LastChancellor = Game.Board.Chancellor;
            Game.Board.Chancellor = null;
            do
            {
                Game.Board.President =
                    Game.Players[(Game.Players.IndexOf(Game.Board.LastPresident) + 1) % Game.Players.Count];
            } while (!Game.Board.President.IsAlive);
            Console.WriteLine($"[{DateTime.Now}] PresidentElectionState, new president is {Game.Board.President.User.Username}");
            Game.State = new ChancellorElectionStartState(Game);
            Game.State.Step();
        }
    }
    class ChancellorElectionStartState : State
    {
        public ChancellorElectionStartState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] ChancellorElectionState begin");
            var except = Game.Players.Where(player => !player.IsAlive || player == Game.Board.President).ToList();
            if (Game.Board.LastPresident is not null && !except.Contains(Game.Board.President))
                except.Add(Game.Board.LastPresident);
            if (Game.Board.LastChancellor is not null && Game.Players.Count <= 5 && !except.Contains(Game.Board.Chancellor))
                except.Add(Game.Board.LastChancellor);
            await Game.SendChoiceAsync(Game.Board.President, except);
            Game.State = new ChancellorElectedState(Game);
        }
    }
    
    class ChancellorElectedState : State
    {
        public ChancellorElectedState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            if (!Game.CandidateForActionId.HasValue)
                throw new Exception("Errors in ellection");
            Game.Board.Chancellor = Game.Players.First(player => player.User.Id == Game.CandidateForActionId.Value);
            Game.CandidateForActionId = null;
            Console.WriteLine($"[{DateTime.Now}] ChancellorElectionState, new chancellor is {Game.Board.Chancellor.User.Username}");
            Game.State = new VotingStartState(Game);
            Game.State.Step();
        }
    }
    
    class VotingStartState : State
    {
        public VotingStartState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            await Game.SendVoteAsync();
            Console.WriteLine($"[{DateTime.Now}] VotingnState");
            Game.State = new VotingState(Game);
        }
    }
    
    class VotingState : State
    {
        public VotingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            if (Game.LastVoteResult is Vote.Undef)
                throw new Exception("Undef after voting???");
            Console.WriteLine($"[{DateTime.Now}] VotingnState result is {Game.LastVoteResult}");
            Game.SendToChatAsync($"Результаты голосования {Game.LastVoteResult}");
            if (Game.LastVoteResult is Vote.Ya)
            {
                Game.Board.ElectionCounter.Clear();
                Game.State = new DrawingCardsState(Game);
            }
            else if (Game.Board.ElectionCounter.Inc())
                Game.State = new ChaosState(Game);
            else            
                Game.State = new PresidentElectionState(Game);

            Game.LastVoteResult = Vote.Undef;
            Game.State.Step();
        }
    }
    
    class ChaosState : State
    {
        public ChaosState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            
        }
    }
    
    class DrawingCardsState : State
    {
        public DrawingCardsState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Game.DraftedLaws = Game.Board.Deck.GetLaws();
            Console.WriteLine($"[{DateTime.Now}] Drafted {Game.DraftedLaws.Count} cards: {string.Join(" ", Game.DraftedLaws)}");
            Game.State = new PresidentDiscardingState(Game);
            Game.State.Step();
        }
    }
    
    class PresidentDiscardingState : State
    {
        public PresidentDiscardingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            await Game.SendPresidentDiscardLawAsync();
            Game.State = new ChancellorChoosingState(Game);
        }
    }

    class ChancellorChoosingState : State
    {
        public ChancellorChoosingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] After discarding - {Game.DraftedLaws.Count} cards: {string.Join(" ", Game.DraftedLaws)}");
            await Game.SendChancellorChooseLawAsync();
            Game.State = new ConfirmingLawState(Game);
        }
    }
    
    class ConfirmingLawState : State
    {
        public ConfirmingLawState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] After choosing - {Game.DraftedLaws.Count} card: {string.Join(" ", Game.DraftedLaws)}");
            // Game.State = new Pres(Game);
        }
    }
}