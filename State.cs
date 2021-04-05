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
                Game.State = new DrawCardsState(Game);
                // check Hitler
                bool isHitler = false;
                int fashlaws = 0; // take fashist law count
                if (isHitler && fashlaws >= 3)
                    Game.State = new EndGameState(Game);
                // show winners
            }
            else
            {
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
            
            // offer veto for chancellor
            bool offer = false;
            // offer = board.fashistlaws == 5
            if (offer)
                Game.State = new OfferVetoState(Game);
            else
                Game.State = new ChooseCardState(Game);
        }
    }
    
    class ChooseCardState : State
    {
        public ChooseCardState(Game game) : base(game)
        { }

        public override async Task Step()
        {
           // take 2 cards
           // chancellor choose one
           Game.State = new AcceptLawState(Game);
        }
    }
    
    class OfferVetoState : State
    {
        public OfferVetoState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // offer to chancellor
            bool agree = true;

            if (agree)
            {
                Game.State = new VetoState(Game);
            }
            else
            {
                Game.State = new ChooseCardState(Game);
            }
        }
    }
    
    class VetoState : State
    {
        public VetoState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // offer to president
            bool agree = true;

            if (agree)
            {
                // discard all cards
                Game.State = new PresidentElectionState(Game);
            }
            else
            {
                Game.State = new ChooseCardState(Game);
            }

        }
    }

    class AcceptLawState : State
    {
        public AcceptLawState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // check count of laws, maybe somebody win
            // go EndGameState
            
                        
            // get current law, accept it
            bool hasAbility = false;
            // take ability
            if (hasAbility)
                Game.State = new UseAbilityState(Game);
            else
                Game.State = new PresidentElectionState(Game);
        }
    }
    
    class UseAbilityState : State
    {
        public UseAbilityState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // use ability
            Game.State = new PresidentElectionState(Game);
            
            // if kill MUST check Hitler here
            // go EndGameState
        }
    }
    
    class EndGameState : State
    {
        public EndGameState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            // inform about game results
            // delete 
        }
    }
}