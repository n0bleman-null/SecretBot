using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public abstract class Strategy
    {
        public static Random Randomizer { get; } = new Random();
        public abstract List<Person> GetRoles(int players);
        public abstract IAbility GetFascistAbility(Counter counter);
        public abstract IAbility GetLiberalAbility(Counter counter);
        public abstract bool HitlerVision();
    }

    public class LowStrategy : Strategy // for 5-6 players
    {
        public override List<Person> GetRoles(int players) => Enumerable
            .Repeat(Person.Liberal, players - Persons.Length)
            .Concat(Persons)
            .OrderBy(a => Randomizer.Next())
            .ToList();

        public override IAbility GetFascistAbility(Counter counter)
            => FascistAbilities[counter.Cur - 1];
        public override IAbility GetLiberalAbility(Counter counter)
            => LiberalAbilities[counter.Cur - 1];

        public override bool HitlerVision() => true;
        
        private static readonly IAbility[] FascistAbilities = new IAbility[]
        {
            new Nothing(),
            new Nothing(),
            new ShowLaws(),
            new Kill(),
            new Kill(),
            new FascistWin()
        };

        private static readonly IAbility[] LiberalAbilities = new IAbility[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new LiberalWin(),
        };

        private static readonly Person[] Persons = new[]
        {
            Person.Hitler,
            Person.Fascist
        };
    }
    
    public class MidStrategy : Strategy // for 5-6 players
    {
        public override List<Person> GetRoles(int players) => Enumerable
            .Repeat(Person.Liberal, players - Persons.Length)
            .Concat(Persons)
            .OrderBy(a => Randomizer.Next())
            .ToList();

        public override IAbility GetFascistAbility(Counter counter)
            => FascistAbilities[counter.Cur - 1];
        public override IAbility GetLiberalAbility(Counter counter)
            => LiberalAbilities[counter.Cur - 1];

        public override bool HitlerVision() => false;
        
        private static readonly IAbility[] FascistAbilities = new IAbility[]
        {
            new Nothing(),
            new CheckRole(),
            new EarlyElections(),
            new Kill(),
            new Kill(),
            new FascistWin()
        };

        private static readonly IAbility[] LiberalAbilities = new IAbility[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new LiberalWin(),
        };

        private static readonly Person[] Persons = new[]
        {
            Person.Hitler,
            Person.Fascist,
            Person.Fascist
        };
    }
    
    public class HardStrategy : Strategy // for 5-6 players
    {
        public override List<Person> GetRoles(int players) => Enumerable
            .Repeat(Person.Liberal, players - Persons.Length)
            .Concat(Persons)
            .OrderBy(a => Randomizer.Next())
            .ToList();

        public override IAbility GetFascistAbility(Counter counter)
            => FascistAbilities[counter.Cur - 1];
        public override IAbility GetLiberalAbility(Counter counter)
            => LiberalAbilities[counter.Cur - 1];

        public override bool HitlerVision() => false;
        
        private static readonly IAbility[] FascistAbilities = new IAbility[]
        {
            new CheckRole(),
            new CheckRole(),
            new EarlyElections(),
            new Kill(),
            new Kill(),
            new FascistWin()
        };

        private static readonly IAbility[] LiberalAbilities = new IAbility[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new LiberalWin(),
        };

        private static readonly Person[] Persons = new[]
        {
            Person.Hitler,
            Person.Fascist,
            Person.Fascist,
            Person.Fascist
        };
    }

}