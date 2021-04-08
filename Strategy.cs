using System;
using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public abstract class Strategy
    {
        public static Random Randomizer { get; } = new Random();

        public List<Person> GetRoles(int players) => Enumerable
            .Repeat(Person.Liberal, players - Persons.Length)
            .Concat(Persons)
            .OrderBy(a => Randomizer.Next())
            .ToList();

        public IAbility GetFascistAbility(Counter counter)
            => FascistAbilities[counter.Cur];

        public IAbility GetLiberalAbility(Counter counter)
            => LiberalAbilities[counter.Cur];

        public bool HitlerVision { get; }
        private IAbility[] FascistAbilities { get; }
        private IAbility[] LiberalAbilities { get; }
        private Person[] Persons { get; }
    }

    public class LowStrategy : Strategy // for 5-6 players
    {
        private static readonly IAbility[] FascistAbilities = new[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
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

        private new static readonly bool HitlerVision = true;
    }

}