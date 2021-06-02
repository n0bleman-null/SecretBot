using System;
using System.Collections.Generic;

#nullable disable

namespace SecretHitlerBot
{
    public partial class ChatDB
    {
        public ChatDB()
        {
            Games = new HashSet<GameDB>();
        }

        public int Id { get; set; }
        public long ChatId { get; set; }

        public virtual ICollection<GameDB> Games { get; set; }
    }
}
