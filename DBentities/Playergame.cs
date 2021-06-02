using System;
using System.Collections.Generic;
using System.Collections;

#nullable disable

namespace SecretHitlerBot
{
    public partial class PlayergameDB
    {
        public int Id { get; set; }
        public int Playerid { get; set; }
        public int Gameid { get; set; }
        public BitArray Role { get; set; }

        public virtual GameDB Game { get; set; }
        public virtual PlayerDB Player { get; set; }
    }
}
