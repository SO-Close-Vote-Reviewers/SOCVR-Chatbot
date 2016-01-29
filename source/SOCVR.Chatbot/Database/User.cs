using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCVR.Chatbot.Database
{
    internal class User
    {
        public User()
        {
            Permissions = new List<UserPermission>();
            ReviewedItems = new List<UserReviewedItem>();
            PermissionsRequested = new List<PermissionRequest>();
            PermissionsReviewed = new List<PermissionRequest>();
        }

        /// <summary>
        /// The Stack Overflow Id number for this user.
        /// </summary>
        public int ProfileId { get; set; }

        /// <summary>
        /// The last time the user has their opt-in/opt-out value changed for review trackings.
        /// If null, then this user has never had the option to be tracked.
        /// </summary>
        public DateTimeOffset? LastTrackingPreferenceChange { get; set; }

        public virtual List<UserPermission> Permissions { get; set; }
        public virtual List<UserReviewedItem> ReviewedItems { get; set; }
        public virtual List<PermissionRequest> PermissionsRequested { get; set; }
        public virtual List<PermissionRequest> PermissionsReviewed { get; set; }
    }
}
