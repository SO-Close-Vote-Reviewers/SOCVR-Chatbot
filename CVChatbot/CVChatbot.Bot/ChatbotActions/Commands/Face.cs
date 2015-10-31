﻿/*
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
using TCL.Extensions;

namespace CVChatbot.Bot.ChatbotActions.Commands
{
    public class Face : UserCommand
    {
        public override string GetActionDescription()
        {
            return "A \"toy command\" for posting a random text face.";
        }

        public override string GetActionName()
        {
            return "Face";
        }

        public override string GetActionUsage()
        {
            return "face";
        }

        public override ActionPermissionLevel GetPermissionLevel()
        {
            return ActionPermissionLevel.Everyone;
        }

        public override void RunAction(Message incommingChatMessage, Room chatRoom, InstallationSettings roomSettings)
        {
            var faces = new[]
            {
                "( ͡° ͜ʖ ͡°)", "¯\\\\_(ツ)_/¯", "༼ つ ◕_◕ ༽つ", "(ง ͠° ͟ل͜ ͡°)ง",
                "ಠ_ಠ", "( ͡°╭͜ʖ╮͡° )", "(ノಠ益ಠ)ノ彡┻━┻", "( ͡° ʖ̯ ͡°)",
                "[̲̅$̲̅(̲̅ ͡° ͜ʖ ͡°̲̅)̲̅$̲̅]", "﴾͡๏̯͡๏﴿ O'RLY?", "༼ つ  ͡° ͜ʖ ͡° ༽つ",
                "(ಥ﹏ಥ)", "| (• ◡•)| (❍ᴥ❍ʋ)", "ლ(ಠ益ಠლ)",
                "(╯°□°)╯︵ ʞooqǝɔɐɟ", "( ͡ᵔ ͜ʖ ͡ᵔ )", "(╯°□°）╯︵ ┻━┻",
                "༼ つ ಥ_ಥ ༽つ", "ᕙ(⇀‸↼‶)ᕗ", "ಠ╭╮ಠ", "ヽ༼ຈل͜ຈ༽ﾉ", "◉_◉",
                "ᕦ(ò_óˇ)ᕤ", "(≧ω≦)", "~(˘▾˘~)", "ᄽὁȍ ̪ őὀᄿ", "┬──┬ ノ( ゜-゜ノ)",
                "( ಠ ͜ʖರೃ)", "┌( ಠ_ಠ)┘", "(ง°ل͜°)ง", "(°ロ°)☝", "(~˘▾˘)~",
                "☜(˚▽˚)☞", "(ಥ_ಥ)", "ლ,ᔑ•ﺪ͟͠•ᔐ.ლ", "(ʘᗩʘ')", "(⊙ω⊙)",
                "⚆ _ ⚆", "°Д°", "ب_ب", "☉_☉", "ಠ~ಠ", "ರ_ರ", "ಠoಠ", "◔ ⌣ ◔",
                "¬_¬", "⊜_⊜", ":)", ":(", ":D", "D:", ":p", ":P", ":o", ":O",
                ":0", ":]", ":[", ":x", ":3", "e.e", "o.o", "O.O", "O.o", "e_e",
                "o_o", "O_O", "O_o", "-_-", "'_'", ">_>", "<_<", "\\o", "o/", "\\o/"
            };

            chatRoom.PostReplyOrThrow(incommingChatMessage, faces.PickRandomTheRightWay());
        }

        protected override string GetRegexMatchingPattern()
        {
            return @"\bface";
        }
    }
}
