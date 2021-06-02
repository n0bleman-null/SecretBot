using System;
using System.Collections.Generic;
using System.Collections;

#nullable disable

namespace SecretHitlerBot
{
    public partial class PlayergameDb
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int GameId { get; set; }
        public bool Role { get; set; }

        public virtual GameDB Game { get; set; }
        public virtual PlayerDB Player { get; set; }
    }
}
