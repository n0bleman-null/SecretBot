using System.Threading.Tasks;

namespace TelegramBot
{
    // TODO REMAKE WITH SCHEME
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
            // if president == null => ellect president
            // else go next president
            Game.State = new PresidentElectionState(Game);
        }
    }
    
    class PresidentElectionState : State
    {
        public PresidentElectionState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // president ellect chancellor
            Game.State = new ChancellorElectionState(Game);
        }
    }
    class ChancellorElectionState : State
    {
        public ChancellorElectionState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // current president ellect chancellor
            Game.State = new VotingState(Game);
        }
    }
    
    class VotingState : State
    {
        public VotingState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            bool most = true;
            // start voting
            if (most)
            {
                // check Hitler
                bool isHitler = false;
                int fashlaws = 0; // take fashist law count
                if (isHitler && fashlaws >= 3)
                    Game.State = new FascistWinState(Game);
                // next
                Game.State = new DrawCardsState(Game);

            }
            else
            {
                //TODO Add CHAOS STATE
                // election counter inc
                // if election counter == 3 go CHAOS
                // else =>
                Game.State = new PresidentElectionState(Game);
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
            // ellecton counter = 0
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