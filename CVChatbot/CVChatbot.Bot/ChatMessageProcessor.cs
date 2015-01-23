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
    public class ChatMessageProcessor
    {
        public delegate void StopBotCommandIssuedHandler();
        public event StopBotCommandIssuedHandler StopBotCommandIssued;

        public void ProcessChatMessage(Message incommingChatMessage, Room chatRoom)
        {
            //do this first so I only have to find the result once per chat message
            bool isReplyToChatbot = MessageIsReplyToChatbot(incommingChatMessage, chatRoom);

            //determine the list of possible actions that work from the message
            var possibleChatbotActionsToRun = ChatbotActionRegister.AllChatActions
                .Where(x => x.DoesChatMessageActiveAction(incommingChatMessage, isReplyToChatbot))
                .ToList();

            if (possibleChatbotActionsToRun.Count > 1)
                throw new Exception("More than one possible chat bot action to run for the input '{0}'"
                    .FormatSafely(incommingChatMessage.Content));

            if (!possibleChatbotActionsToRun.Any())
            {
                //didn't find an action to run, what to do next depends of if was
                //a reply to the chatbot or not
                if (isReplyToChatbot)
                {
                    //user was trying to make a command
                    SendUnrecognizedCommandToDatabase(incommingChatMessage.GetContentsWithStrippedMentions());
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "Sorry, I don't understand that. Use `{0}` for a list of commands."
                        .FormatInline(ChatbotActionRegister.GetChatBotActionUsage<Commands>()));
                }
                //else it's a trigger, do nothing

                return;
            }

            //you have a single item to run
            var chatbotActionToRun = possibleChatbotActionsToRun.Single();

            //now, do you have permission to run it?
            if (DoesUserHavePermissionToRunAction(chatbotActionToRun, incommingChatMessage.AuthorID))
            {
                //have permission, run it
                RunChatbotAction(chatbotActionToRun, incommingChatMessage, chatRoom);
            }
            else
            {
                //don't have permission, tell the user only if it's a command
                if (isReplyToChatbot)
                {
                    chatRoom.PostReplyOrThrow(incommingChatMessage, "Sorry, you need more permissions to run that command.");
                }
                //don't do anything for triggers
            }
        }

        private void SendUnrecognizedCommandToDatabase(string command)
        {
            using (CVChatBotEntities db = new CVChatBotEntities())
            {
                var newEntry = new UnrecognizedCommand
                {
                    Command = command
                };

                db.UnrecognizedCommands.Add(newEntry);
                db.SaveChanges();
            }
        }

        private bool DoesUserHavePermissionToRunAction(ChatbotAction actionToRun, int chatUserId)
        {
            var neededPermissionLevel = actionToRun.GetPermissionLevel();

            if (neededPermissionLevel == ActionPermissionLevel.Everyone)
                return true;

            //now you need to look up the person in the database
            using (CVChatBotEntities db = new CVChatBotEntities())
            {
                var dbUser = db.RegisteredUsers
                    .Where(x => x.ChatProfileId == chatUserId)
                    .SingleOrDefault();

                if (dbUser == null) //at this point, the permission is Registered or Owner, 
                    return false;    //and if the user is not in the database at all then it can't work

                if (neededPermissionLevel == ActionPermissionLevel.Registered)
                    return true; //the user is in the list, that's all we need to check

                if (dbUser.IsOwner && neededPermissionLevel == ActionPermissionLevel.Owner)
                    return true;
            }

            //fall past the last check (for owner), so default to "no"
            return false;
        }

        private void RunChatbotAction(ChatbotAction action, Message incommingChatMessage, Room chatRoom)
        {
            //record as started
            var id = RunningCommandsManager.MarkCommandAsStarted(
                action.GetActionName(),
                incommingChatMessage.AuthorName,
                incommingChatMessage.AuthorID);

            try
            {
                action.RunAction(incommingChatMessage, chatRoom);

                //if the command was "stop bot", need to trigger a program shutdown
                if (action is StopBot)
                {
                    if (StopBotCommandIssued != null)
                        StopBotCommandIssued();
                }
            }
            catch (Exception ex)
            {
                //ChatMessageProcessor is responsible for outputting any errors that occur
                //while running chatbot actions. Anything outside of the RunAction() method
                //should be handled higher up
                TellChatAboutErrorWhileRunningAction(ex, chatRoom, action);
            }

            RunningCommandsManager.MarkCommandAsFinished(id);
        }

        private void TellChatAboutErrorWhileRunningAction(Exception ex, Room chatRoom, ChatbotAction actionToRun)
        {
            var headerLine = "I hit an error while trying to run '{0}':"
                .FormatSafely(actionToRun.GetActionName());

            var errorMessage = "    " + ex.FullErrorMessage(Environment.NewLine + "    ");

            var stackTraceMessage = ex.GetAllStackTraces();

            var secondMessage = errorMessage + Environment.NewLine +
                "    ----" + Environment.NewLine +
                stackTraceMessage;

            chatRoom.PostMessageOrThrow(headerLine);
            chatRoom.PostMessageOrThrow(secondMessage);
        }

        private bool MessageIsReplyToChatbot(Message chatMessage, Room chatRoom)
        {
            if (chatMessage.ParentID == -1)
                return false;

            var parentMessage = chatRoom.GetMessage(chatMessage.ParentID);
            return parentMessage.AuthorID == chatRoom.Me.ID;
        }
    }
}
