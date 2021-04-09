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
            Game.State = new PresidentElectionState(Game);
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
            Game.State = new ChancellorElectionState(Game);
        }
    }
    class ChancellorElectionState : State
    {
        public ChancellorElectionState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            var except = Game.Players.Where(player => !player.IsAlive || player == Game.Board.President).ToList();
            if (!except.Contains(Game.Board.President))
                except.Add(Game.Board.LastPresident);
            if (Game.Players.Count <= 5 && !except.Contains(Game.Board.Chancellor))
                except.Add(Game.Board.LastChancellor);
            await Game.SendChoiceAsync(Game.Board.President, except);
            Game.Board.Chancellor = Game.Players.First(player => player.User.Id == Game.CandidateForActionId.Value);
            Game.State = new VotingState(Game);
        }
    }
    
    class VotingState : State
    {
        public VotingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            await Game.SendVoteAsync();
            switch (Game.LastVoteResult)
            {
                case Vote.Ya:
                    if (Game.Board.FascistLawsCounter.Cur >= 3 && Game.Board.Chancellor.Person == Person.Hitler)
                        Game.State = new FascistWinState(Game);
                    else
                        Game.State = new DrawCardsState(Game);
                    break;
                case Vote.Nein:
                    if (Game.Board.ElectionCounter.Inc())
                        Game.State = new ChaosState(Game);
                    else
                        Game.State = new PresidentElectionState(Game);
                    break;
                case Vote.Undef:
                    throw new Exception("Голосование не завершилось???");
                    break;
            }
        }
    }
    
    class ChaosState : State
    {
        public ChaosState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // take card
            // go accept law
            Game.State = new AcceptLawState(Game);
        }
    }

    
    class DrawCardsState : State
    {
        public DrawCardsState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            
            // draw 3 cards
            // president discard one
            Game.State = new ChooseCardState(Game);
        }
    }
    
    class ChooseCardState : State
    {
        public ChooseCardState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // offer veto to cancellor
            bool veto = false;
            if (veto)
            {
                // offer to president
                bool agree = true;

                if (agree)
                {
                    // discard all cards
                    Game.State = new PresidentElectionState(Game);
                    // don't increase ellection counter
                }
                else
                {
                    Game.State = new ChooseCardState(Game);
                }
            }
           // take 2 cards
           // chancellor choose one
           Game.State = new AcceptLawState(Game);
        }
    }

    class AcceptLawState : State
    {
        public AcceptLawState(Game game) : base(game)
        { }

        public override async Task Step()
        {

            Game.State = new PresidentElectionState(Game);
            // election counter = 0
        }
    }

    class FascistWinState : State
    {
        public FascistWinState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // inform about game results
            // delete 
        }
    }
    
    class LiberalWinState : State
    {
        public LiberalWinState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // inform about game results
            // delete 
        }
    }
}