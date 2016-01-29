using System;
using System.Linq;
using ChatExchangeDotNet;
using TCL.Extensions;
using SOCVR.Chatbot.ChatbotActions;
using SOCVR.Chatbot.ChatbotActions.Commands;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;
using CVChatbot.Bot;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace SOCVR.Chatbot.ChatRoom
{
    /// <summary>
    /// This class is responsible for determining whether to act on an incoming chat message or not,
    /// then performing that action.
    /// </summary>
    public class ChatMessageProcessor
    {
        private Regex yesReply;
        private ConcurrentDictionary<Message, KeyValuePair<Message, ChatbotAction>> unrecdCmds;
        private SimilarCommand simCmd;

        public ChatMessageProcessor()
        {
            simCmd = new SimilarCommand();
            unrecdCmds = new ConcurrentDictionary<Message, KeyValuePair<Message, ChatbotAction>>();
            yesReply = new Regex(@"(?i)^y([eu]+s+|[ue]+p|eah?)\b", RegexOptions.Compiled | RegexOptions.CultureInvariant);
        }

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
            var isReplyToChatbot = false;
            ChatbotAction chatbotActionToRun = null;

            // Is the message a confirmation to a command suggestion?
            if (yesReply.IsMatch(incomingChatMessage.Content))
            {
                var cmd = unrecdCmds.FirstOrDefault(kv => kv.Value.Key.ID == incomingChatMessage.ParentID &&
                    kv.Key.Author.ID == incomingChatMessage.Author.ID);

                if (cmd.Key != null)
                {
                    chatbotActionToRun = cmd.Value.Value;
                    KeyValuePair<Message, ChatbotAction> temp;
                    unrecdCmds.TryRemove(cmd.Key, out temp);
                }
            }

            if (chatbotActionToRun == null)
            {
                // Do this first so I only have to find the result once per chat message.
                isReplyToChatbot = MessageIsReplyToChatbot(incomingChatMessage, chatRoom);

                // Determine the list of possible actions that work from the message.
                var possibleChatbotActionsToRun = ChatbotActionRegister.AllChatActions
                    .Where(x => x.DoesChatMessageActiveAction(incomingChatMessage, isReplyToChatbot))
                    .ToList();

                if (possibleChatbotActionsToRun.Count > 1)
                {
                    throw new Exception("More than one possible chat bot action to run for the input '{0}'"
                        .FormatSafely(incomingChatMessage.Content));
                }

                if (!possibleChatbotActionsToRun.Any())
                {
                    // Didn't find an action to run, what to do next depends of if the message was
                    // a reply to the chatbot or not.
                    if (isReplyToChatbot)
                    {
                        var similarCommand = simCmd.FindCommand(incomingChatMessage.Content);
                        var msg = "Sorry, I don't understand that. ";

                        if (similarCommand != null)
                        {
                            msg += $"Did you mean `{similarCommand.ActionUsage}`?";

                            var reply = chatRoom.PostReply(incomingChatMessage, msg);
                            if (reply == null)
                            {
                                throw new InvalidOperationException("Unable to post message");
                            }

                            unrecdCmds[incomingChatMessage] = new KeyValuePair<Message, ChatbotAction>(reply, similarCommand);
                        }
                    }

                    // Else it's a trigger, do nothing.
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
                    chatRoom.PostReplyOrThrow(incomingChatMessage,
                        $"Sorry, you are not in the {chatbotActionToRun.RequiredPermissionGroup.ToString()} permission group. Do you want to request access? (reply with 'yes')");
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
            //if the command does not require a permission group to run
            if (actionToRun.RequiredPermissionGroup == null)
            {
                return true;
            }

            //command requires permission. look up user in database

            using (var db = new DatabaseContext())
            {
                var dbUser = db.Users
                    .Include(x => x.Permissions)
                    .SingleOrDefault(x => x.ProfileId == chatUserId);

                if (dbUser == null)
                {
                    //this user does not exist in the database, and because this command requires permission
                    //we have to deny usage
                    return false;
                }

                //check if the user is in the necessary permission group
                var userPermissionGroups = dbUser.Permissions
                    .Select(x => x.PermissionGroup);

                if (!actionToRun.RequiredPermissionGroup.Value.In(userPermissionGroups))
                {
                    //user is not in the required permission group
                    return false;
                }
                else
                {
                    //user is in the required permission group
                    return true;
                }
            }
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
