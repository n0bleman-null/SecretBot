using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace SecretHitlerBot
{
    public partial class GameDB
    {
        public GameDB()
        {
            Playergames = new HashSet<PlayergameDb>();
        }

        [Key]
        public int Id { get; set; }
        public int ChatId { get; set; }
        public bool Winner { get; set; }

        public virtual ChatDB Chat { get; set; }
        public virtual ICollection<PlayergameDb> Playergames { get; set; }
    }
}
