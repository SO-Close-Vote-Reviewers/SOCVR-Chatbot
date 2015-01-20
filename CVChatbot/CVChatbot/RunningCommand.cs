using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot
{
    public class RunningCommand
    {
        public string CommandName { get; set; }
        public string RunningForUserName { get; set; }
        public int RunningForUserId { get; set; }
        public DateTimeOffset CommandStartTs { get; set; }
    }
}
