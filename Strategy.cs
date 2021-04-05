using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public abstract class Strategy
    {
        public static uint MaxElections { get; } = 3;
        public static List<Role> Roles { get; }
        public static bool HitlerVision { get; }
        public IAbility[] FascistAbilities { get; }
        public IAbility[] LiberalAbilities { get; }
    }

    public class LowStrategy : Strategy // for 5-6 players
    {
        public readonly List<Role> Roles = new List<Role>()
        {
            Role.Fascist, Role.Fascist
        };

        public readonly IAbility[] FascistAbilities = new IAbility[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing()
        };

        public readonly IAbility[] LiberalAbilities = new IAbility[]
        {
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing(),
            new Nothing()
        };
        
        public readonly bool HitlerVision = true;
    }
    
    
}