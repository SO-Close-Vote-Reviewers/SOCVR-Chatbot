using System;
using System.Linq;
using ChatExchangeDotNet;
using TCL.Extensions;
using SOCVR.Chatbot.ChatbotActions;
using SOCVR.Chatbot.ChatbotActions.Commands;

namespace SOCVR.Chatbot.ChatRoom
{
    /// <summary>
    /// This class is responsible for determining whether to act on an incoming chat message or not,
    /// then performing that action.
    /// </summary>
    public class ChatMessageProcessor
    {
        public ChatMessageProcessor() { }

        public delegate void StopBotCommandIssuedHandler(object sender, EventArgs e);
        public event StopBotCommandIssuedHandler StopBotCommandIssued;

        /// <summary>
        /// Main entry point for the class.
        /// Takes a message revived from chat, determines what action should be taken, then performs that action.
        /// </summary>
        /// <param name="incomingChatMessage">The chat message that was said.</param>
        /// <param name="chatRoom">The room the chat message was said in.</param>
        public void ProcessChatMessage(Message incomingChatMessage, Room chatRoom)
        {
            // Do this first so I only have to find the result once per chat message.
            bool isReplyToChatbot = MessageIsReplyToChatbot(incomingChatMessage, chatRoom);

            // Determine the list of possible actions that work from the message.
            var possibleChatbotActionsToRun = ChatbotActionRegister.AllChatActions
                .Where(x => x.DoesChatMessageActiveAction(incomingChatMessage, isReplyToChatbot))
                .ToList();

            if (possibleChatbotActionsToRun.Count > 1)
                throw new Exception("More than one possible chat bot action to run for the input '{0}'"
                    .FormatSafely(incomingChatMessage.Content));

            if (!possibleChatbotActionsToRun.Any())
            {
                // Didn't find an action to run, what to do next depends of if the message was
                // a reply to the chatbot or not.
                if (isReplyToChatbot)
                {
                    // User was trying to make a command.
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "Sorry, I don't understand that. Use `{0}` for a list of commands."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>()));
                }
                // Else it's a trigger, do nothing.

                return;
            }

            // You have a single item to run.
            var chatbotActionToRun = possibleChatbotActionsToRun.Single();

            // Now, do you have permission to run it? If you are a mod the answer is yes, else you need to check.
            if (incomingChatMessage.Author.IsMod || DoesUserHavePermissionToRunAction(chatbotActionToRun, incomingChatMessage.Author.ID))
            {
                // Have permission, run it.
                RunChatbotAction(chatbotActionToRun, incomingChatMessage, chatRoom);
            }
            else
            {
                // Don't have permission, tell the user only if it's a command.
                if (isReplyToChatbot)
                {
                    chatRoom.PostReplyOrThrow(incomingChatMessage, "Sorry, you need more permissions to run that command.");
                }
                // Don't do anything for triggers.
            }
        }

        /// <summary>
        /// Determines if the specified chat user has the correct permissions to run the chatbot action.
        /// </summary>
        /// <param name="actionToRun">The action the user would like to run.</param>
        /// <param name="chatUserId">The chat user who initiated this request.</param>
        /// <returns></returns>
        private bool DoesUserHavePermissionToRunAction(ChatbotAction actionToRun, int chatUserId)
        {
            throw new NotImplementedException();

            //var neededPermissionLevel = actionToRun.PermissionLevel;

            //// If the permission level of the action is "everyone" then just return true.
            //// Don't need to do anything else, like searching though the database.
            //if (neededPermissionLevel == ActionPermissionLevel.Everyone)
            //    return true;

            //// Now you need to look up the person in the database
            //var userRecordInDB = da.GetRegisteredUserByChatProfileId(chatUserId);

            //if (userRecordInDB == null) // At this point, the permission is Registered or Owner,
            //    return false;    // and if the user is not in the database at all then it can't work.

            //if (neededPermissionLevel == ActionPermissionLevel.Registered)
            //    return true; // The user is in the list, that's all we need to check.

            //if (userRecordInDB.IsOwner && neededPermissionLevel == ActionPermissionLevel.Owner)
            //    return true;

            //// Fall past the last check (for owner), so default to "no".
            //return false;
        }

        /// <summary>
        /// Determines if the chat message is directed to the chatbot or not.
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="chatRoom"></param>
        /// <returns></returns>
        private bool MessageIsReplyToChatbot(Message chatMessage, Room chatRoom)
        {
            if (chatMessage.ParentID == -1)
                return false;

            // Check if we're trying to fetch a deleted message.
            try
            {
                var parentMessage = chatRoom.GetMessage(chatMessage.ParentID);
                return parentMessage.Author.ID == chatRoom.Me.ID;
            }
            catch (MessageNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Runs the logic for the chatbot action and records the start and stop of the action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="incomingChatMessage"></param>
        /// <param name="chatRoom"></param>
        private void RunChatbotAction(ChatbotAction action, Message incomingChatMessage, Room chatRoom)
        {
            // Record as started.
            var id = RunningChatbotActionsManager.MarkChatbotActionAsStarted(
                action.ActionName,
                incomingChatMessage.Author.Name,
                incomingChatMessage.Author.ID);

            try
            {
                action.RunAction(incomingChatMessage, chatRoom);

                // If the command was "stop bot", need to trigger a program shutdown.
                if (action is StopBot)
                {
                    if (StopBotCommandIssued != null)
                        StopBotCommandIssued(this, new EventArgs());
                }
            }
            catch (Exception ex)
            {
                // ChatMessageProcessor is responsible for outputting any errors that occur
                // while running chatbot actions. Anything outside of the RunAction() method
                // should be handled higher up.
                TellChatAboutErrorWhileRunningAction(ex, chatRoom, action);
            }

            // Mark as finished.
            RunningChatbotActionsManager.MarkChatbotActionAsFinished(id);
        }

        /// <summary>
        /// Call this method if you get an error while running a ChatbotAction.
        /// This will attempt to send a message to chat about error in a standard format.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="chatRoom"></param>
        /// <param name="actionToRun"></param>
        private void TellChatAboutErrorWhileRunningAction(Exception ex, Room chatRoom, ChatbotAction actionToRun)
        {
            var headerLine = $"I hit an error while trying to run '{actionToRun.ActionName}':";

            var errorMessage = "    " + ex.FullErrorMessage(Environment.NewLine + "    ");

            var stackTraceMessage = ex.GetAllStackTraces();

            var detailsLine = errorMessage + Environment.NewLine +
                "    ----" + Environment.NewLine +
                stackTraceMessage;

            chatRoom.PostMessageOrThrow(headerLine);
            chatRoom.PostMessageOrThrow(detailsLine);
        }
    }
}
