using System;
using System.Linq;
using System.Threading.Tasks;
using SecretHitlerBot;
using Telegram.Bot.Types.ReplyMarkups;
namespace TelegramBot
{
    
    public abstract class State
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

        public override async Task Step() // TODO transform all Game.Player.Where(p => p.IsAlive) to Game.Alive ...
        {
            Game.Strategy = Game.Players.Count switch
            {
                5 => new LowStrategy(),
                6 => new LowStrategy(),
                7 => new MidStrategy(),
                8 => new MidStrategy(),
                9 => new HardStrategy(),
                10 => new HardStrategy(),
                _ => throw new Exception("Error in players.count")
            };
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
            foreach (var player in Game.Players)
            {
                await player.SendMessageAsync($"Ваша партыя: {player.Role switch {Role.Fascist => "фашыст", Role.Liberal => "лiберал"}}\n" +
                                              $"Ваша роля: {player.Person switch {Person.Fascist => "фашыст",Person.Liberal => "лiберал",Person.Hitler => "гiтлер" }}\n");
                if (player.Role is Role.Fascist)
                {
                    if (player.Person is Person.Hitler && !Game.Strategy.HitlerVision())
                        continue;
                    player.SendMessageAsync(
                        $"Фашысты: {string.Join(", ", Game.Players.Where(p => p.Person == Person.Fascist).Select(p => p.User.Username))}\n" +
                        $"Гітлер: {Game.Players.First(h => h.Person == Person.Hitler).User.Username}");
                }
            }
            await Game.SendToChatAsync($"Гульня пачалася");
            Game.State = new PresidentElectionState(Game);
            await Game.State.Step();
            Console.WriteLine($"[{DateTime.Now}] Laws in deck: {string.Join("->", Game.Board.Deck.Laws)}");
            Console.WriteLine($"[{DateTime.Now}] President queue: {string.Join("->", Game.Players.Select(p => p.User.Username))}");
        }
    }
    
    class PresidentElectionState : State
    {
        public PresidentElectionState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] PresidentElectionState begin");
            if (Game.EarlyElection)
            {
                Game.EarlyElection = false;
                Game.Board.LastPresident = Game.EarlyElectedPresident;
                Game.EarlyElectedPresident = null;
            }
            else
            {
                Game.Board.LastPresident = Game.Board.President;
            }
            Game.Board.LastChancellor = Game.Board.Chancellor;
            Game.Board.Chancellor = null;
            var alive = Game.Players.Where(p => p.IsAlive).ToList();
            Game.Board.President =
                alive.ElementAt((alive.IndexOf(Game.Board.LastPresident) + 1) % alive.Count);
            Console.WriteLine(
                $"[{DateTime.Now}] PresidentElectionState, new president is {Game.Board.President}");
            await Game.SendToChatAsync($"Iнфармацыя\n" +
                                       $"Новы прэзідэнт: {Game.Board.President}\n" +
                                       $"Чарга выбараў: {string.Join("->", Game.Players.Where(p => p.IsAlive).Select(p => p.User.Username))}\n" +
                                       $"Лічыльнік галасаванняў: {Game.Board.ElectionCounter.Cur}\n" +
                                       $"Фашысцкіх законаў прынята: {Game.Board.FascistLawsCounter.Cur}\n" +
                                       $"Ліберальных законаў прынята: {Game.Board.LiberalLawsCounter.Cur}\n" +
                                       $"Забітыя гульцы: {string.Join(", ", Game.Players.Where(p => !p.IsAlive))}\n" +
                                       $"Ня гітлер: {string.Join(", ", Game.Players.Where(p => !p.MaybeHitler && p.IsAlive))}");
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
            var except = Game.Players.Where(player => !player.IsAlive || player == Game.Board.President || player == Game.Board.LastChancellor || player == Game.Board.LastPresident).ToList();
            if (Game.Board.LastPresident is not null && Game.Players.Count <= 5 && Game.Board.LastPresident != Game.Board.LastChancellor && Game.Board.LastPresident != Game.Board.President && Game.Board.LastPresident.IsAlive)
                except.Remove(Game.Board.LastPresident);
            await Game.SendChoiceAsync(Game.Board.President, except, $"Выберыце канцлера з прапанаваных гульцоў");
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
            await Game.SendToChatAsync($"Новым канцлерам прызначаны {Game.Board.Chancellor}");
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
            await Game.SendVoteAsync($"Вы згодныя з выбарам прэзідэнта і канцлера?");
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
            Console.WriteLine($"[{DateTime.Now}] VotingState result is {Game.LastVoteResult}, EllectionCounter - {Game.Board.ElectionCounter.Cur}");
            Game.SendToChatAsync($"Па выніках галасавання новы ўрад {Game.LastVoteResult switch{Vote.Ya => "приняты", Vote.Nein => "адкінут"}}");
            switch (Game.LastVoteResult)
            {
                case Vote.Ya:
                    Game.Board.ElectionCounter.Clear(); // TODO move to law confirmed
                    if (Game.Board.FascistLawsCounter.Cur > 2)
                    {
                        if (Game.Board.Chancellor.Person is Person.Hitler)
                        {
                            await new FascistWin().Execute(Game);
                            return;
                        }
                        Game.Board.Chancellor.MaybeHitler = false;
                    }
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
            await Game.SendToChatAsync(
                $"Краіна пагрузілася ў хаос, прыняты {Game.DraftedLaws.First() switch {Law.Fascist => "фашысцкі", Law.Liberal => "ліберальны"}} закон");
            Console.WriteLine($"[{DateTime.Now}] Confirmed law: {string.Join(" ", Game.DraftedLaws)}");
            switch (Game.DraftedLaws.First())
            {
                case Law.Fascist:
                    Game.Board.FascistLawsCounter.Inc();
                    if (Game.Strategy.GetFascistAbility(Game.Board.FascistLawsCounter) is FascistWin)
                    {
                        await Game.Strategy.GetFascistAbility(Game.Board.FascistLawsCounter).Execute(Game);
                        return;
                    }
                    break;
                case Law.Liberal:
                    Game.Board.LiberalLawsCounter.Inc();
                    if (Game.Strategy.GetLiberalAbility(Game.Board.LiberalLawsCounter) is LiberalWin)
                    {
                        await Game.Strategy.GetLiberalAbility(Game.Board.LiberalLawsCounter).Execute(Game);
                        return;
                    }
                    break;
            }
            Console.WriteLine($"[{DateTime.Now}] Facsist counter - {Game.Board.FascistLawsCounter.Cur} | Liberal counter - {Game.Board.LiberalLawsCounter.Cur}");
            Game.State = new PresidentElectionState(Game);
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
            await Game.SendPresidentDiscardLawAsync($"Выберыце закон, які хочаце адхіліць");
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
            await Game.SendChancellorChooseLawAsync($"Выберыце закон, які хочаце прыняць");
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
                    await Game.SendToChatAsync($"Канцлер ужыў права вета");
                    await Game.SendVetoRequestAsync(Game.Board.President);
                    Game.State = new VetoState(Game);
                    break;
                case Vote.Nein:
                    Game.LastVoteResult = Vote.Undef;
                    await Game.SendToChatAsync($"Канцлер не скарыстаўся правам вета");
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
                    Game.Board.ElectionCounter.Inc();
                    await Game.SendToChatAsync($"Прэзідэнт пацвердзіў права вета");
                    Game.State = new PresidentElectionState(Game);
                    await Game.State.Step();
                    break;
                case Vote.Nein:
                    Game.State = new ChancellorChoosingState(Game);
                    await Game.SendToChatAsync($"Прэзідэнт адхіліў права вета");
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
            await Game.SendToChatAsync($"Прыняты {Game.DraftedLaws.First() switch {Law.Fascist => "фашысцкі", Law.Liberal => "ліберальны"}} закон");
            switch (Game.DraftedLaws.First())
            {
                case Law.Fascist:
                    Game.Board.FascistLawsCounter.Inc();
                    var fascistAbility = Game.Strategy.GetFascistAbility(Game.Board.FascistLawsCounter);
                    await fascistAbility.Execute(Game);
                    break;
                case Law.Liberal:
                    Game.Board.LiberalLawsCounter.Inc();
                    var liberalAbility = Game.Strategy.GetLiberalAbility(Game.Board.LiberalLawsCounter);
                    await liberalAbility.Execute(Game);
                    break;
            }
            Console.WriteLine($"[{DateTime.Now}] Facsist counter - {Game.Board.FascistLawsCounter.Cur} | Liberal counter - {Game.Board.LiberalLawsCounter.Cur}");
        }
    }
    
    class RoleCheckAbilityState : State
    {
        public RoleCheckAbilityState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            if (!Game.CandidateForActionId.HasValue)
                throw new Exception("Errors in role check id");
            await Game.Board.President.SendMessageAsync(
                $"Яго роля {Game.Players.First(player => player.User.Id == Game.CandidateForActionId.Value).Role switch {Role.Fascist => "фашыст", Role.Liberal => "лiберал"}}");
            Game.CandidateForActionId = null;
            Game.State = new PresidentElectionState(Game);
            await Game.State.Step();
        }
    }
    
    class EarlyElectionState : State
    {
        public EarlyElectionState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            if (!Game.CandidateForActionId.HasValue)
                throw new Exception("Errors in early election id");
            var pl = Game.Players.First(player => player.User.Id == Game.CandidateForActionId.Value);
            Game.CandidateForActionId = null;

            Game.Board.President = pl;
            Game.Board.LastChancellor = Game.Board.Chancellor;
            Game.Board.Chancellor = null;
            await Game.SendToChatAsync($"Часовым прэзідэнтам прызначаны {pl}");
            await Game.SendToChatAsync($"Iнфармацыя\n" +
                                       $"Часовы прэзідэнт: {Game.Board.President}\n" +
                                       $"Лічыльнік галасаванняў: {Game.Board.ElectionCounter.Cur}\n" +
                                       $"Фашысцкіх законаў прынята: {Game.Board.FascistLawsCounter.Cur}\n" +
                                       $"Ліберальных законаў прынята: {Game.Board.LiberalLawsCounter.Cur}\n" +
                                       $"Забітыя гульцы: {string.Join(", ", Game.Players.Where(p => !p.IsAlive))}\n" +
                                       $"Ня гітлер: {string.Join(", ", Game.Players.Where(p => !p.MaybeHitler && p.IsAlive))}");
            Game.State = new ChancellorElectionStartState(Game);
            await Game.State.Step();
        }
    }
    
    class KillAbilityState : State
    {
        public KillAbilityState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            if (!Game.CandidateForActionId.HasValue)
                throw new Exception("Errors in kill id");
            var pl = Game.Players.First(player => player.User.Id == Game.CandidateForActionId.Value);
            pl.IsAlive = false;
            Game.CandidateForActionId = null;
            if (pl.Person is Person.Hitler)
            {
                await Game.SendToChatAsync($"{pl} быў забіты, ён быў Гітлерам");
                await new LiberalWin().Execute(Game);
                return;
            }
            pl.MaybeHitler = false;

            await Game.SendToChatAsync($"{pl.User.Username} быў забіты, ён не быў Гітлерам");
            Game.State = new PresidentElectionState(Game);
            Console.WriteLine($"[{DateTime.Now}] Kill confirmed - {pl.User.Username}");
            await Game.State.Step();
        }
    }
    
    class EndGameState : State
    {
        public EndGameState(Game game) : base(game)
        { }

        public override async Task Step()
        {
            Console.WriteLine($"[{DateTime.Now}] Endgame, game status is {Game.GameStatus}");
            
            using (var db = new SechitContext())
            {
                var entry = db.Games.Add(new GameDB
                {
                    ChatId = db.Chats.Single(c => c.ChatId == Game.ChatId).Id,
                    Winner = Game.GameStatus switch
                    {
                        GameStatus.LiberalWin => false, 
                        GameStatus.FascistWin => true
                    }
                });
                db.SaveChanges();
                foreach (var player in Game.Players)
                {
                    db.Playergames.Add(new PlayergameDb 
                    {
                        PlayerId = db.Players.Single(p => p.PlayerId == player.User.Id).Id,
                        GameId = entry.Entity.Id, 
                        Role = player.Role switch
                        {
                            Role.Liberal => false,
                            Role.Fascist => true
                        }
                    });
                }
                db.SaveChanges();
            }
            
            Games.Instance.Remove(Game.ChatId);
        }
    }
}