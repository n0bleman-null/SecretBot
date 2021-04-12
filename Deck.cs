using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public enum Law
    {
        Liberal,
        Fascist
    }
    public class Deck
    {
        private uint _count = 17;

        public uint Count
        {
            get => _count;
            private set
            {
                if (value <= 5)
                {
                    _count = 17;
                    Laws = Laws.Concat(Discarded.OrderBy(law => Strategy.Randomizer.Next())).ToList();
                    Discarded.Clear();
                }
                _count = value;
            }
        }
        private List<Law> Laws { get; set; } =
            Enumerable.Repeat(Law.Fascist, 11).Concat(Enumerable.Repeat(Law.Liberal, 6))
                .OrderBy(law => Strategy.Randomizer.Next()).ToList();

        private List<Law> Discarded { get; set; } = new List<Law>(17);

        public List<Law> GetLaws() // 3 Laws
        {
            var l = Laws.Take(3);
            Count -= 3;
            Discarded.AddRange(l);
            Laws.RemoveRange(0,3);
            return l.ToList();
        }

        public List<Law> GetLaw()
        {
            var l = Laws.Take(1);
            Discarded.AddRange(l);
            Count--;
            Laws.RemoveAt(0);
            return l.ToList();
        }
    }
}