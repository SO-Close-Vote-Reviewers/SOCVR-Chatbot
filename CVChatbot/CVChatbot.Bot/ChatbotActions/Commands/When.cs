/*
 * CVChatbot. Chatbot for the SO Close Vote Reviewers Chat Room.
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





using System;
using System.Globalization;
using ChatExchangeDotNet;
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class When : UserCommand
    {
        public override string GetActionDescription()
        {
            return "A \"toy command\" for getting a random date.";
        }

        public override string GetActionName()
        {
            return "When";
        }

        public override string GetActionUsage()
        {
            return "when will ...";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            var phrases = new[]
            {
                "Tomorrow.",
                "Yesterday.",
                "Within a week.",
                "Within a month.",
                "Next year.",
                "In 3... 2... 1...",
                "Yes.",
                "In 6 to 8 moons."
            };
            var r = new Random(DateTime.UtcNow.Millisecond);
            var message = "";

            if (r.NextDouble() > 0.5)
            {
                // Pick any date within 10 years from now.
                var date = DateTime.UtcNow.Add(TimeSpan.FromDays(r.Next(-3650, 3650)));
                message = date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
            }
            else
            {
                message = phrases.PickRandom();
            }

            chatRoom.PostReplyOrThrow(incommingChatMessage, message);
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"^when.*";
        }
    }
}
