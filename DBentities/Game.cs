using System;
using System.Collections.Generic;
using System.Collections;

#nullable disable

namespace SecretHitlerBot
{
    public partial class GameDB
    {
        public GameDB()
        {
            Playergames = new HashSet<PlayergameDB>();
        }

        public int Id { get; set; }
        public int Chatid { get; set; }
        public BitArray Winner { get; set; }

        public virtual ChatDB Chat { get; set; }
        public virtual ICollection<PlayergameDB> Playergames { get; set; }
    }
}
