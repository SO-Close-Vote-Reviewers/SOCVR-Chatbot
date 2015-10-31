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





using ChatExchangeDotNet;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Panic : UserCommand
    {
        private string[] gifs = new string[] 
        {
            "http://rack.0.mshcdn.com/media/ZgkyMDEzLzA2LzE4LzdjL0JlYWtlci4zOWJhOC5naWYKcAl0aHVtYgkxMjAweDk2MDA-/4a93e3c4/4a4/Beaker.gif",
            "http://i1094.photobucket.com/albums/i442/PeetaEverdeen/OHSHITRUNAROUND.gif",
            "http://rack.0.mshcdn.com/media/ZgkyMDEzLzA2LzE4L2I2L0pvaG5ueURlcHBwLmM1YjNkLmdpZgpwCXRodW1iCTEyMDB4OTYwMD4/70417de1/fe5/Johnny-Depp-panics.gif",
            "http://tech.graze.com/content/images/2014/Apr/colbert-panic.gif",
            "http://media.giphy.com/media/HZs7JJYJ6rdqo/giphy.gif"
        };

        public override string GetActionDescription()
        {
            return "A \"toy command\" for posting an appropriate gif.";
        }

        public override string GetActionName()
        {
            return "Panic";
        }

        public override string GetActionUsage()
        {
            return "panic!!!";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            chatRoom.PostReplyOrThrow(incommingChatMessage, gifs.PickRandomTheRightWay());
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"\s?panic[!1.]*$";
        }
    }
}
