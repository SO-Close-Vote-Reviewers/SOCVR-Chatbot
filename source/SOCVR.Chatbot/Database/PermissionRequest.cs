using System;

namespace SOCVR.Chatbot.Database
{
    internal class PermissionRequest
    {
        public int Id { get; set; }

        /// <summary>
        /// The user that is asking for the permission.
        /// </summary>
        public User RequestingUser { get; set; }
        public int RequestingUserId { get; set; }

        /// <summary>
        /// The user that either approved or rejected the permission.
        /// </summary>
        public User ReviewingUser { get; set; }
        public int? ReviewingUserId { get; set; }

        public DateTimeOffset RequestedOn { get; set; }

        public PermissionGroup RequestedPermissionGroup { get; set; }

        /// <summary>
        /// True = accepted,
        /// False = rejected,
        /// Null = not processed
        /// </summary>
        public bool? Accepted { get; set; }
    }
}
