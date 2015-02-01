using ChatExchangeDotNet;
using CVChatbot.Bot.ChatbotActions;
using CVChatbot.Bot.ChatbotActions.Commands;
using CVChatbot.Bot.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot
{
    /// <summary>
    /// This class is responsible for determining whether to act on an incoming chat message or not,
    /// then performing that action.
    /// </summary>
    public class ChatMessageProcessor
    {
        public delegate void StopBotCommandIssuedHandler(object sender, EventArgs e);
        public event StopBotCommandIssuedHandler StopBotCommandIssued;

        /// <summary>
        /// Main entry point for the class.
        /// Takes a message revived from chat, determines what action should be taken, then performs that action.
        /// </summary>
        /// <param name="incommingChatMessage">The chat message that was said.</param>
        /// <param name="chatRoom">The room the chat message was said in.</param>
        public void ProcessChatMessage(Message incommingChatMessage, Room chatRoom)
        {
            // Do this first so I only have to find the result once per chat message.
            bool isReplyToChatbot = MessageIsReplyToChatbot(incommingChatMessage, chatRoom);

            // Determine the list of possible actions that work from the message.
            var possibleChatbotActionsToRun = ChatbotActionRegister.AllChatActions
                .Where(x => x.DoesChatMessageActiveAction(incommingChatMessage, isReplyToChatbot))
                .ToList();

            if (possibleChatbotActionsToRun.Count > 1)
                throw new Exception("More than one possible chat bot action to run for the input '{0}'"
                    .FormatSafely(incommingChatMessage.Content));

            if (!possibleChatbotActionsToRun.Any())
            {
                // Didn't find an action to run, what to do next depends of if the message was
                // a reply to the chatbot or not.
                if (isReplyToChatbot)
                {
                    // User was trying to make a command.
                    SendUnrecognizedCommandToDatabase(incommingChatMessage.GetContentsWithStrippedMentions());
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "Sorry, I don't understand that. Use `{0}` for a list of commands."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>()));
                }
                // Else it's a trigger, do nothing.

                return;
            }

            // You have a single item to run.
            var chatbotActionToRun = possibleChatbotActionsToRun.Single();

            // Bow, do you have permission to run it?
            if (DoesUserHavePermissionToRunAction(chatbotActionToRun, incommingChatMessage.AuthorID))
            {
                // Have permission, run it.
                RunChatbotAction(chatbotActionToRun, incommingChatMessage, chatRoom);
            }
            else
            {
                // Don't have permission, tell the user only if it's a command.
                if (isReplyToChatbot)
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "Sorry, you need more permissions to run that command.");
                }
                // Don't do anything for triggers.
            }
        }

        /// <summary>
        /// Takes a command (already stripped of mentions and trimmed) and inserts it into
        /// a table of commands that the chatbot did not recognize.
        /// </summary>
        /// <param name="command"></param>
        private void SendUnrecognizedCommandToDatabase(string command)
        {
            using (var db = new CVChatBotEntities())
            {
                var newEntry = new UnrecognizedCommand
                {
                    Command = command
                };

                db.UnrecognizedCommands.Add(newEntry);
                db.SaveChanges();
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
            var neededPermissionLevel = actionToRun.GetPermissionLevel();

            // If the permission level of the action is "everyone" then just return true.
            // Don't need to do anything else, like searching though the database.
            if (neededPermissionLevel == ActionPermissionLevel.Everyone)
                return true;

            // Now you need to look up the person in the database
            using (var db = new CVChatBotEntities())
            {
                var dbUser = db.RegisteredUsers.SingleOrDefault(x => x.ChatProfileId == chatUserId);

                if (dbUser == null) // At this point, the permission is Registered or Owner,
                    return false;    // and if the user is not in the database at all then it can't work.

                if (neededPermissionLevel == ActionPermissionLevel.Registered)
                    return true; // The user is in the list, that's all we need to check.

                if (dbUser.IsOwner && neededPermissionLevel == ActionPermissionLevel.Owner)
                    return true;
            }

            // Fall past the last check (for owner), so default to "no".
            return false;
        }

        /// <summary>
        /// Runs the logic for the chatbot action and records the start and stop of the action.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="incommingChatMessage"></param>
        /// <param name="chatRoom"></param>
        private void RunChatbotAction(ChatbotAction action, Message incommingChatMessage, Room chatRoom)
        {
            // Record as started.
            var id = RunningChatbotActionsManager.MarkChatbotActionAsStarted(
                action.GetActionName(),
                incommingChatMessage.AuthorName,
                incommingChatMessage.AuthorID);

            try
            {
                action.RunAction(incommingChatMessage, chatRoom);

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
            var headerLine = "I hit an error while trying to run '{0}':"
                .FormatSafely(actionToRun.GetActionName());

            var errorMessage = "    " + ex.FullErrorMessage(Environment.NewLine + "    ");

            var stackTraceMessage = ex.GetAllStackTraces();

            var detailsLine = errorMessage + Environment.NewLine +
                "    ----" + Environment.NewLine +
                stackTraceMessage;

            chatRoom.PostMessageOrThrow(headerLine);
            chatRoom.PostMessageOrThrow(detailsLine);
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

            var parentMessage = chatRoom.GetMessage(chatMessage.ParentID);
            return parentMessage.AuthorID == chatRoom.Me.ID;
        }
    }
}
