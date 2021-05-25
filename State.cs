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
            await Game.State.Step();
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
            await Game.State.Step();
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
            await Game.State.Step();
        }
    }
    
    class VotingStartState : State
    {
        public VotingStartState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            await Game.SendVoteAsync();
            Console.WriteLine($"[{DateTime.Now}] VotingState begin");
            Game.State = new VotingState(Game);
        }
    }
    
    class VotingState : State
    {
        public VotingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] VotingState result is {Game.LastVoteResult}");
            Game.SendToChatAsync($"Результаты голосования {Game.LastVoteResult}");
            switch (Game.LastVoteResult)
            {
                case Vote.Ya:
                    Game.Board.ElectionCounter.Clear(); // move to law confirmed
                    Game.State = new DrawingCardsState(Game);
                    break;
                case Vote.Nein:
                    if (Game.Board.ElectionCounter.Inc())
                        Game.State = new ChaosState(Game);
                    else            
                        Game.State = new PresidentElectionState(Game);
                    break;
                case Vote.Undef:
                    throw new Exception("Undef after voting???");
                    break;
            }
            Game.LastVoteResult = Vote.Undef;
            await Game.State.Step();
        }
    }
    
    class ChaosState : State
    {
        public ChaosState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] ChaosState");
            Game.DraftedLaws = Game.Board.Deck.GetLaw();
            Game.State = new ConfirmingLawState(Game);
            await Game.State.Step();
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
            await Game.State.Step();
        }
    }
    
    class PresidentDiscardingState : State
    {
        public PresidentDiscardingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] ChaosState");
            await Game.SendPresidentDiscardLawAsync();
            if (Game.Board.FascistLawsCounter.Cur >= 5)
                Game.State = new ChancellorVetoOfferState(Game);
            else 
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
    
    class ChancellorVetoOfferState : State
    {
        public ChancellorVetoOfferState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] ChancellorVetoOfferState");
            await Game.SendVetoRequestAsync(Game.Board.Chancellor);
            Game.State = new PresidentVetoOfferState(Game);
        }
    }
    
    class PresidentVetoOfferState : State
    {
        public PresidentVetoOfferState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] PresidentVetoOfferState");
            switch (Game.LastVoteResult)
            {
                case Vote.Ya:
                    Game.LastVoteResult = Vote.Undef;
                    await Game.SendVetoRequestAsync(Game.Board.President);
                    Game.State = new PresidentVetoOfferState(Game);
                    break;
                case Vote.Nein:
                    Game.LastVoteResult = Vote.Undef;
                    Game.State = new ChancellorChoosingState(Game);
                    await Game.State.Step();
                    break;
                case Vote.Undef:
                    throw new Exception("Error in chancellor veto offer");
                    break;
            }
        }
    }
    
    class VetoState : State
    {
        public VetoState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] VetoState");
            switch (Game.LastVoteResult)
            {
                case Vote.Ya:
                    Game.State = new PresidentElectionState(Game);
                    await Game.State.Step();
                    break;
                case Vote.Nein:
                    Game.State = new ChancellorChoosingState(Game);
                    await Game.State.Step();
                    break;
                case Vote.Undef:
                    throw new Exception("Error in chancellor veto offer");
                    break;
            }
            Game.LastVoteResult = Vote.Undef;
        }
    }
    
    class ConfirmingLawState : State
    {
        public ConfirmingLawState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] Confirmed law: {string.Join(" ", Game.DraftedLaws)}");
            switch (Game.DraftedLaws.First())
            {
                case Law.Fascist:
                    Game.Board.FascistLawsCounter.Inc();
                    // Game.Strategy.GetFascistAbility(Game.Board.FascistLawsCounter);
                    break;
                case Law.Liberal:
                    Game.Board.LiberalLawsCounter.Inc();
                    break;
            }
        }
    }
}