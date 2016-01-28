using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCVR.Chatbot.Database
{
    internal class UserPermission
    {
        public int Id { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }

        public PermissionGroup PermissionGroup { get; set; }
    }

    internal enum PermissionGroup
    {
        Reviewer,
        BotOwner
    }
}
