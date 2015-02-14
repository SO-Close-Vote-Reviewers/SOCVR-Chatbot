using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Bot.Database
{
    class RegisteredUser
    {
        public int Id { get; set; }
        public int ChatProfileId { get; set; }
        public bool IsOwner { get; set; }
    }
}
