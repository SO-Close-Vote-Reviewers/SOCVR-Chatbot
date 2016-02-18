using System;

namespace SOCVR.Chatbot.Database
{
    internal class UserPermission
    {
        public int Id { get; set; }

        public virtual User User { get; set; }
        public int UserId { get; set; }

        public PermissionGroup PermissionGroup { get; set; }

        public DateTimeOffset JoinedOn { get; set; }
    }

    internal enum PermissionGroup
    {
        Reviewer,
        BotOwner
    }
}
