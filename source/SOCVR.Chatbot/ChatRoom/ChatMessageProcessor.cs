using System;
using System.Linq;
using ChatExchangeDotNet;
using TCL.Extensions;
using SOCVR.Chatbot.ChatbotActions;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;
using CVChatbot.Bot;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using SOCVR.Chatbot.ChatbotActions.Commands.Admin;
using SOCVR.Chatbot.ChatbotActions.Commands.Permission;
using SOCVR.Chatbot.PassiveActions;

namespace SOCVR.Chatbot.ChatRoom
{
    /// <summary>
    /// This class is responsible for determining whether to act on an incoming chat message or not,
    /// then performing that action.
    /// </summary>
    public class ChatMessageProcessor
    {
        private Regex yesReply;
        private ConcurrentDictionary<Message, KeyValuePair<Message, string>> unrecdCmds;
        private SimilarCommand simCmd;

        public ChatMessageProcessor()
        {
            simCmd = new SimilarCommand();
            unrecdCmds = new ConcurrentDictionary<Message, KeyValuePair<Message, string>>();
            yesReply = new Regex(@"(?i)^y([eu]+s+|[ue]+p|eah?)\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

        public delegate void StopBotCommandIssuedHandler(object sender, EventArgs e);
        public event StopBotCommandIssuedHandler StopBotCommandIssued;

        public void ProcessNewMessage(Message incomingChatMessage, Room chatRoom)
        {
            //before checking for active actions, check for passive actions
            var autoViewRequestsAction = new AutoPostViewRequests();
            if (autoViewRequestsAction.ShouldActionBeRan(incomingChatMessage))
            {
                autoViewRequestsAction.RunAction(incomingChatMessage, chatRoom);
            }
        }

        /// <summary>
        /// Main entry point for the class.
        /// Takes a message revived from chat, determines what action should be taken, then performs that action.
        /// </summary>
        /// <param name="incomingChatMessage">The chat message that was said.</param>
        /// <param name="chatRoom">The room the chat message was said in.</param>
        public void ProcessPing(Message incomingChatMessage, Room chatRoom)
        {
            var isReplyToChatbot = false;
            ChatbotAction chatbotActionToRun = null;

            EnsureAuthorInDatabase(incomingChatMessage);

            // Is the message a confirmation to a command suggestion?
            if (yesReply.IsMatch(incomingChatMessage.Content))
            {
                var cmd = unrecdCmds.FirstOrDefault(kv => kv.Value.Key.ID == incomingChatMessage.ParentID &&
                    kv.Key.Author.ID == incomingChatMessage.Author.ID);

                if (cmd.Key != null)
                {
                    // What's a good sign of laziness? ..... Using reflection.
                    typeof(Message)
                        .GetProperty("Content")
                        .SetValue(incomingChatMessage, cmd.Value.Value);

                    KeyValuePair<Message, string> temp;
                    unrecdCmds.TryRemove(cmd.Key, out temp);
                }
            }

            if (chatbotActionToRun == null)
            {
                // Determine the list of possible actions that work from the message.
                var possibleChatbotActionsToRun = ChatbotActionRegister.AllChatActions
                    .Where(x => x.DoesChatMessageActiveAction(incomingChatMessage, true))
                    .ToList();

                if (possibleChatbotActionsToRun.Count > 1)
                {
                    throw new Exception("More than one possible chat bot action to run for the input '{0}'"
                        .FormatSafely(incomingChatMessage.Content));
                }

                if (!possibleChatbotActionsToRun.Any())
                {
                    var results = simCmd.FindCommand(incomingChatMessage.Content);
                    var msg = "Sorry, I don't understand that. ";

                    if (results != null)
                    {
                        if (!results.OptionsSubstituted)
                        {
                            msg += $"Maybe you meant `{results.SuggestedCmdText}`.";
                        }
                        else
                        {
                            msg += $"Did you mean `{results.SuggestedCmdText}`?";
                        }

                        var reply = chatRoom.PostReply(incomingChatMessage, msg);
                        if (reply == null)
                        {
                            throw new InvalidOperationException("Unable to post message");
                        }

                        if (results.OptionsSubstituted)
                        {
                            unrecdCmds[incomingChatMessage] = new KeyValuePair<Message, string>(reply, results.SuggestedCmdText);
                        }
                    }

                    return;
                }

                // You have a single item to run.
                chatbotActionToRun = possibleChatbotActionsToRun.Single();
            }

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
                    //a person can be denied a command for one of two reasons:
                    // 1) they are not in the required permission group
                    // 2) they are not in ANY permission groups
                    // this is dependent on the command they try to run

                    if (chatbotActionToRun.RequiredPermissionGroup != null)
                    {
                        //the command required a specific group which the user was not a part of
                        chatRoom.PostReplyOrThrow(incomingChatMessage,
                            $"Sorry, you are not in the {chatbotActionToRun.RequiredPermissionGroup} permission group. You can request access by running `request permission to {chatbotActionToRun.RequiredPermissionGroup}`.");
                    }
                    else
                    {
                        //the command can be ran by anyone who is in at least one permission group,
                        //but this user is not in any
                        chatRoom.PostReplyOrThrow(incomingChatMessage,
                            $"Sorry, you need to be in at least one permission group to run this command. Run `{ChatbotActionRegister.GetChatBotActionUsage<Membership>()}` to see the list of groups.");
                    }
                }
                // Don't do anything for triggers.
            }
        }

        /// <summary>
        /// Before we test the message, we should add the user who wrote the
        /// message to the database. This guarantees that the message author 
        /// exists in the database, so the commands don't need to do that lookup
        /// </summary>
        /// <param name="incomingChatMessage"></param>
        private void EnsureAuthorInDatabase(Message incomingChatMessage)
        {
            using (var db = new DatabaseContext())
            {
                db.EnsureUserExists(incomingChatMessage.Author.ID);

                if (incomingChatMessage.Author.IsMod)
                {
                    db.EnsureUserIsInAllPermissionGroups(incomingChatMessage.Author.ID);
                }
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
            //if the command does not require the user to be in any permission groups
            if (actionToRun.UserMustBeInAnyPermissionGroupToRun == false)
            {
                //this is a "public" command
                return true;
            }

            //command requires permission. look up user in database

            using (var db = new DatabaseContext())
            {
                var dbUser = db.Users
                    .Include(x => x.Permissions)
                    .Single(x => x.ProfileId == chatUserId);

                //there are two configurations for the permission required for the command:
                // 1) a specific group is required
                // 2) any group is required

                if (actionToRun.RequiredPermissionGroup != null)
                {
                    //the user must be in the named group in order to run
                    var userPermissionGroups = dbUser.Permissions
                        .Select(x => x.PermissionGroup);

                    return actionToRun.RequiredPermissionGroup.Value.In(userPermissionGroups);
                }
                else
                {
                    //the user must be in ANY permission group to run
                    return dbUser.Permissions.Any();
                }
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

