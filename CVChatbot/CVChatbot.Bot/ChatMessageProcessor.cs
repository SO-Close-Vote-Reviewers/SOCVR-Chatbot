using ChatExchangeDotNet;
using CVChatbot.Bot.ChatbotActions;
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
        public void ProcessChatMessage(Message incommingChatMessage, Room chatRoom)
        {
            //do this first so I only have to find the result once per chat message
            bool isReplyToChatbot = MessageIsReplyToChatbot(incommingChatMessage, chatRoom);

            //determine the list of possible actions that work from the message
            var possibleChatbotActionsToRun = ChatbotActionRegister.ChatActions
                .Where(x => x.DoesChatMessageActiveAction(incommingChatMessage, isReplyToChatbot))
                .ToList();

            if (possibleChatbotActionsToRun.Count > 1)
                throw new Exception("More than one possible chat bot action to run");

            if (!possibleChatbotActionsToRun.Any())
            {
                //didn't find an action to run, what to do next depends of if was
                //a reply to the chatbot or not
                if (isReplyToChatbot)
                {
                    //user was trying to make a command
                    chatRoom.PostReply(incommingChatMessage, "Sorry, I don't understand that.");
                }
                //else it's a trigger, do nothing

                return;
            }

            //you have a single item to run, attempt to run it
            var chatbotActionToRun = possibleChatbotActionsToRun.Single();
            RunChatbotAction(chatbotActionToRun, incommingChatMessage, chatRoom);
        }

        private void RunChatbotAction(ChatbotAction action, Message incommingChatMessage, Room chatRoom)
        {
            //record as started
            var id = RunningCommandsManager.MarkCommandAsStarted(
                action.GetCommandName(),
                incommingChatMessage.AuthorName,
                incommingChatMessage.AuthorID);

            try
            {
                action.RunAction(incommingChatMessage, chatRoom);
            }
            catch (Exception ex)
            {
                TellChatAboutErrorWhileRunningAction(ex, chatRoom);
            }

            RunningCommandsManager.MarkCommandAsFinished(id);
        }

        private void TellChatAboutErrorWhileRunningAction(Exception ex, Room chatRoom)
        {
            var errorMessage = "    " + ex.FullErrorMessage(Environment.NewLine + "    ");
            chatRoom.PostMessage("OH GOD IT BROKE! EVERYTHING IS ON FIRE!");
            chatRoom.PostMessage(errorMessage);
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
