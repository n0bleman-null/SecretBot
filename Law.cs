namespace TelegramBot
{
    public interface ILaw
    {
        abstract IAbility GetNextAbility(Strategy strategy, Counter lawcounter);
    }

    public class FacsistLaw : ILaw // TODO solve this shit
    {
        public IAbility GetNextAbility(Strategy strategy, Counter lawcounter)
            => strategy.FascistAbilities[lawcounter.Cur];
    }

}