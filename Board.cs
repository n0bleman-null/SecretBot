using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public class Board
    {
        public Player President { get; set; } = null;
        public Player LastPresident { get; set; } = null;
        public Player Chancellor { get; set; } = null;
        public Player LastChancellor { get; set; } = null;

        public ElectionCounter ElectionCounter { get; set; } = new ElectionCounter();
        public LawsCounter LiberalLawsCounter { get; set; } = new LawsCounter();
        public LawsCounter FascistLawsCounter { get; set; } = new LawsCounter();

        public Deck Deck { get; set; } = new Deck();
        // TODO realize DECK
    }
}