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





using ChatExchangeDotNet;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    /// <summary>
    /// A User Command is a message sent directly to the chatbot which wants the chatbot to perform an action.
    /// </summary>
    public abstract class UserCommand : ChatbotAction
    {
        /// <summary>
        /// Takes the contents from the message, strips out any "mentions", and trims the sides of the string.
        /// </summary>
        /// <param name="incommingMessage"></param>
        /// <returns></returns>
        protected override sealed string GetMessageContentsReadyForRegexParsing(Message incommingMessage)
        {
            return incommingMessage
                .GetContentsWithStrippedMentions()
                .Trim();
        }

        /// <summary>
        /// Hard-coded to return true.
        /// If you want to run a User Command it must be a reply to the chatbot.
        /// </summary>
        /// <returns></returns>
        protected override bool GetMessageIsReplyToChatbotRequiredValue()
        {
            return true;
        }
    }
}
