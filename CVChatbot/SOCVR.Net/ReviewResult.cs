using System;



namespace SOCVRDotNet
{
    /// <summary>
    /// This class holds data regarding a user's actions taken during a review.
    /// </summary>
    public class ReviewResult
    {
        /// <summary>
        /// The username of the user that took the specified action.
        /// </summary>
        public string UserName { get; private set; }

        /// <summary>
        /// The action taken by the user.
        /// </summary>
        public ReviewAction Action { get; private set; }

        /// <summary>
        /// Date/time information when the review action was taken.
        /// </summary>
        public DateTime TimeStamp { get; private set; }



        public ReviewResult(string userName, ReviewAction action, DateTime timeStamp)
        {
            UserName = userName;
            Action = action;
            TimeStamp = timeStamp;
        }
    }
}
