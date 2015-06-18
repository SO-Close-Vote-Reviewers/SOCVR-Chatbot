/*
 * ChatExchange.Net. Chatbot for the SO Close Vote Reviewers Chat Room.
 * Copyright © 2015, SO-Close-Vote-Reviewers.
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */





namespace CVChatbot.Bot.ChatbotActions.Triggers
{
    /// <summary>
    /// A Trigger is a message said in chat which is not a direct reply to the chatbot, but the chatbot will take action on the message.
    /// </summary>
    public abstract class Trigger : ChatbotAction
    {
        /// <summary>
        /// Hard-coded to return false.
        /// If you are running a trigger then it can't be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected override sealed bool GetMessageIsReplyToChatbotRequiredValue()
        {
            return false;
        }

        /// <summary>
        /// Takes the content from the message and trims the contents. It does not remove or replace anything within the string.
        /// </summary>
        /// <param name="incommingMessage"></param>
        /// <returns></returns>
        protected override string GetMessageContentsReadyForRegexParsing(ChatExchangeDotNet.Message incommingMessage)
        {
            return incommingMessage.Content
                .Trim();
        }
    }
}
