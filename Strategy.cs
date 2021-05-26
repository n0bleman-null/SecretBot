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
        public bool HitlerVision { get; }
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

        private static bool HitlerVision { get; } = true;
        
        private static readonly IAbility[] FascistAbilities = new IAbility[]
        {
            new EarlyElections(),
            new CheckRole(),
            new ShowLaws(),
            new CheckRole(),
            new Nothing(),
            new Nothing()
        };

        private static readonly IAbility[] LiberalAbilities = new[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
        };

        private static readonly Person[] Persons = new[]
        {
            Person.Hitler,
            Person.Fascist
        };
    }

}