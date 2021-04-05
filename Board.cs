using System.Collections.Generic;
using System.Linq;

namespace TelegramBot
{
    public class Board
    {
        public Player President { get; set; }
        public Player LastPresident { get; set; }
        public Player Chancellor { get; set; }
        public Player LastChancellor { get; set; }

        public Counter ElectionCounter { get; set; } = new ElectionCounter();
        public Counter LiberalLawsCounter { get; set; } = new LiberalLawsCounter();
        public Counter FascistLawsCounter { get; set; } = new FascistLawsCounter();
        // TODO write logic
    }
}