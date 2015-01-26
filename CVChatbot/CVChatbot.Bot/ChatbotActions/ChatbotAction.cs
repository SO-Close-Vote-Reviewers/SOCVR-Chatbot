﻿using ChatExchangeDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CVChatbot.Bot.ChatbotActions
{
    public abstract class ChatbotAction
    {
        public bool DoesChatMessageActiveAction(Message incomingMessage, bool isMessageAReplyToChatbot)
        {
            //first, check if the message is a reply or not and if the Action accepts that
            var requiredIsReplyValue = GetMessageIsReplyToChatbotRequiredValue();

            if (isMessageAReplyToChatbot != requiredIsReplyValue)
                return false;

            //now regex test it
            var formattedContents = GetMessageContentsReadyForRegexParsing(incomingMessage);
            var regex = GetRegexMatchingObject();

            return regex.IsMatch(formattedContents);
        }

        /// <summary>
        /// The DoesChatMessageActiveAction method passes in if the incoming message is 
        /// a reply to the chatbot. This method tells what that value must be for the 
        /// action to be activated. For example, User Commands must be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected abstract bool GetMessageIsReplyToChatbotRequiredValue();

        protected abstract string GetRegexMatchingPattern();

        protected Regex GetRegexMatchingObject()
        {
            var pattern = GetRegexMatchingPattern();
            return new Regex(pattern, RegexOptions.IgnoreCase);
        }

        protected abstract string GetMessageContentsReadyForRegexParsing(Message incommingMessage);

        public abstract void RunAction(Message incommingChatMessage, Room chatRoom);

        public abstract string GetActionName();

        public abstract string GetActionDescription();

        public abstract string GetActionUsage();

        public abstract ActionPermissionLevel GetPermissionLevel();
    }

    public enum ActionPermissionLevel
    {
        /// <summary>
        /// All people who join the chat room are allowed to run this command or activate the trigger
        /// </summary>
        Everyone,

        /// <summary>
        /// Only people in the tracked users list can run this command or activate the trigger
        /// </summary>
        Registered,

        /// <summary>
        /// Only people in the tracked users list who are labeled as "owner" can run this command or activate the trigger
        /// </summary>
        Owner,
    }
}