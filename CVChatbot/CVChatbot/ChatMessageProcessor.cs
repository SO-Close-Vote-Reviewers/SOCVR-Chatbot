using ChatExchangeDotNet;
using CVChatbot.Commands;
using CVChatbot.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;
using CVChatbot.Model;

namespace CVChatbot
{
    /// <summary>
    /// A class to take a chat message and acts on it if meets certain criteria
    /// </summary>
    public class ChatMessageProcessor
    {
        //private UserCommandProcessor ucp;
        private List<UserCommand> userCommands;
        private List<Trigger> triggers;

        public ChatMessageProcessor()
        {
            userCommands = new List<UserCommand>();
            triggers = new List<Trigger>();

            AddUserCommand<Alive>();
            AddUserCommand<Help>();
            AddUserCommand<Status>();
            AddUserCommand<Stats>();
            AddUserCommand<StartingSession>();
            AddUserCommand<TrackUser>();
            AddUserCommand<LastSessionStats>();
            AddUserCommand<LastSessionEditCount>();

            AddTrigger<CompletedAudit>();
            AddTrigger<EmptyFilter>();
            AddTrigger<OutOfReviewActions>();
            AddTrigger<OutOfCloseVotes>();
        }

        private void AddUserCommand<TUserCommand>()
            where TUserCommand : UserCommand, new()
        {
            userCommands.Add(new TUserCommand());
        }

        private void AddTrigger<TTrigger>()
            where TTrigger : Trigger, new()
        {
            triggers.Add(new TTrigger());
        }

        public async Task ProcessChatMessageAsync(Message chatMessage, Room chatRoom)
        {
            await Task.Run(() => ProcessChatMessage(chatMessage, chatRoom));
        }

        private bool IsChatUserARegisteredUser(int userChatId)
        {
            using (SOChatBotEntities db = new SOChatBotEntities())
            {
                return db.RegisteredUsers
                    .Where(x => x.ChatProfileId == userChatId)
                    .Any();
            }
        }

        private bool DoesUserHavePermissionToRunAction<TAction>(int chatUserId, TAction actionToRun)
            where TAction : ChatbotAction
        {
            var neededPermissionLevel = actionToRun.GetPermissionLevel();

            if (neededPermissionLevel == ActionPermissionLevel.Everyone)
                return true;

            //now you need to look up the person in the database
            using (SOChatBotEntities db = new SOChatBotEntities())
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

        private void ProcessChatMessage(Message chatMessage, Room chatRoom)
        {
            bool isReplyToChatbot = MessageIsReplyToChatbot(chatMessage, chatRoom);

            if (isReplyToChatbot)
            {
                ProcessInputAsUserCommand(chatMessage, chatRoom);
            }
            else
            {
                ProcessInputAsTrigger(chatMessage, chatRoom);
            }
        }

        private void ProcessInputAsTrigger(Message chatMessage, Room chatRoom)
        {
            //check if there is a trigger that matches
            var triggerToRun = GetTrigger(chatMessage);
            if (triggerToRun != null)
            {
                if (DoesUserHavePermissionToRunAction(chatMessage.AuthorID, triggerToRun))
                {
                    triggerToRun.RunTrigger(chatMessage, chatRoom);
                }
                //else, ignore (don't complain about permissions for triggers)
            }

            //else, do nothing
        }

        private void ProcessInputAsUserCommand(Message chatMessage, Room chatRoom)
        {
            //check if it's a command
            var userCommandToRun = GetUserCommand(chatMessage);
            if (userCommandToRun != null)
            {
                if (DoesUserHavePermissionToRunAction(chatMessage.AuthorID, userCommandToRun))
                {
                    userCommandToRun.RunCommand(chatMessage, chatRoom);
                }
                else
                {
                    chatRoom.PostReply(chatMessage, "Sorry, you need more permissions to run that command.");
                }
            }
            else
            {
                chatRoom.PostReply(chatMessage, "Sorry, I don't understand that.");
            }
        }

        /// <summary>
        /// Determines which user command to run based on the chat message.
        /// Message must be addressed to the chat bot.
        /// Returns null if no command could be found.
        /// </summary>
        /// <param name="chatMessage"></param>
        /// <param name="chatRoom"></param>
        /// <returns></returns>
        private UserCommand GetUserCommand(Message chatMessage)
        {
            var possibleUserCommands = userCommands
                .Where(x => x.DoesInputTriggerCommand(chatMessage))
                .ToList();

            if (possibleUserCommands.Count > 1)
                throw new Exception("Found more than one user command for the input '{0}'"
                    .FormatSafely(chatMessage.Content));

            return possibleUserCommands.SingleOrDefault();
        }

        private Trigger GetTrigger(Message chatMessage)
        {
            var possibleTriggers = triggers
                .Where(x => x.DoesInputActivateTrigger(chatMessage))
                .ToList();

            if (possibleTriggers.Count > 1)
                throw new Exception("Found more than one trigger for the input '{0}'"
                    .FormatSafely(chatMessage.Content));

            return possibleTriggers.SingleOrDefault();
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
