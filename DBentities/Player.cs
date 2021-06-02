using System;
using System.Collections.Generic;

#nullable disable

namespace SecretHitlerBot
{
    public partial class PlayerDB
    {
        public PlayerDB()
        {
            Playergames = new HashSet<PlayergameDB>();
        }

        public int Id { get; set; }
        public long Playerid { get; set; }

        public virtual ICollection<PlayergameDB> Playergames { get; set; }
    }
}
