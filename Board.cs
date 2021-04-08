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

        public Counter ElectionCounter { get; set; } = new ElectionCounter();
        public Counter LiberalLawsCounter { get; set; } = new LawsCounter();
        public Counter FascistLawsCounter { get; set; } = new LawsCounter();
        // TODO write logic
    }
}