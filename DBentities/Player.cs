using System;
using System.Collections.Generic;

#nullable disable

namespace SecretHitlerBot
{
    public partial class PlayerDB
    {
        public PlayerDB()
        {
            Playergames = new HashSet<PlayergameDb>();
        }

        public int Id { get; set; }
        public long PlayerId { get; set; }

        public virtual ICollection<PlayergameDb> Playergames { get; set; }
    }
}
