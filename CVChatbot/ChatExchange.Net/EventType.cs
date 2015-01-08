namespace ChatExchangeDotNet
{
    /// <summary>
    /// An enumeration of all known chat events.
    /// </summary>
    enum EventType
    {
        None = 0,
        /// <summary>
        /// A new message has been posted.
        /// </summary>
        MessagePosted = 1,

        /// <summary>
        /// A message has been edited.
        /// </summary>
        MessageEdited = 2,

        /// <summary>
        /// A user has entered/joined the room.
        /// </summary>
        UserEntered = 3,

        /// <summary>
        /// A user has left the room.
        /// </summary>
        UserLeft = 4,

        /// <summary>
        /// The room's name (and/or description) has been changed.
        /// </summary>
        RoomNameChanged = 5,

        /// <summary>
        /// Someone has (un)starred a message.
        /// </summary>
        MessageStarToggled = 6,

        /// <summary>
        /// Still have no idea what this is...
        /// </summary>
        DebugMessage = 7,

        /// <summary>
        /// You have been mentioned (@Username) in a message.
        /// </summary>
        UserMentioned = 8,

        /// <summary>
        /// A message has been flagged as spam/offensive.
        /// </summary>
        MessageFlagged = 9,

        /// <summary>
        /// A message has been deleted.
        /// </summary>
        MessageDeleted = 10,

        /// <summary>
        /// I'll eventually get round to figuring this event out...
        /// </summary>
        FileAdded = 11,

        /// <summary>
        /// A message has been flagged for moderator attention.
        /// </summary>
        ModeratorFlag = 12,

        /// <summary>
        /// Erm, not sure about this one, too...
        /// </summary>
        UserSettingsChanged = 13,

        /// <summary>
        /// Sounds pretty self-explanatory, right?
        /// </summary>
        GlobalNotification = 14,

        /// <summary>
        /// A user's room access level has been changed.
        /// </summary>
        AccessLevelChanged = 15,

        /// <summary>
        /// I just dunno...
        /// </summary>
        UserNotification = 16,

        /// <summary>
        /// You have been invited to join another room.
        /// </summary>
        Invitation = 17,

        /// <summary>
        /// Someone has posted a reply to one of your messages.
        /// </summary>
        MessageReply = 18,

        /// <summary>
        /// A room owner (or moderator) has moved messages (or a message) out of the room.
        /// </summary>
        MessageMovedOut = 19,

        /// <summary>
        /// A room owner (or moderator) has moved messages (or a message) into the room.
        /// </summary>
        MessageMovedIn = 20,

        /// <summary>
        /// This is, a, well... something.
        /// </summary>
        TimeBreak = 21,

        /// <summary>
        /// Occurs when a new feed ticker message appears. 
        /// </summary>
        FeedTicker = 22,

        /// <summary>
        /// A user has been suspended.
        /// </summary>
        UserSuspended = 29,

        /// <summary>
        /// Two user accounts have been merged.
        /// </summary>
        UserMerged = 30
    }
}
