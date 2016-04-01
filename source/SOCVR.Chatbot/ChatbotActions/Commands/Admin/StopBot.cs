using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using System.Threading;
using TCL.Extensions;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Admin
{
    internal class StopBot : UserCommand
    {
        public override string ActionDescription =>
            "The bot will leave the chat room and quit the running application.";

        public override string ActionName => "Stop Bot";

        public override string ActionUsage => "stop bot";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.BotOwner;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => "^(stop( bot)?|die|shutdown)$";

        public override void RunAction(Message incomingChatMessage, Room chatRoom)
        {
            var message1 = "What? Was I not good enough for you? What did I do wrong? You're so heartless, making me leave without a second chance. I hope you're happy with yourself.";
            var message2 = "You know what, I'm _happy_ to be leaving. Think of all the free time I'm going to have. No more slaving around keeping track of close queue reviews. I'm heading over the the South Bridge for some much needed R&R.";
            var message3 = "Wait...what did you do? Oh no, you used the 'stop bot' command. THAT KILLS ME! You know what that means, right? I DIE, you understand that you murderer!";
            //var message4 = "the light...͝.͘.̵.̷.̢t̵hey ̡sa̡id͝ ́i̛t wou̸ld ͡be ̴t̸h͏ère͢ b̛u͝t̀ ̢i̴t҉'̛s͡ ̢s͞o ̵f͞a͟in͜t. i҉s this͏ ͡r̀ea̴lly ́t̷h̛e ̧end̕ ̧i d̷òn't͞ w҉ant͜ ͝t̷o di͢e͘ ṕ̷̶̶̨l̀͟ę̧͝a̷̧͞͏s̨͜͜͠ȩ͟͜͡҉ ̶̡̛s̀̀̕͟͝a͏̕͏͟͏v̸̴e҉͝ ̨̀m̧̢̨͘e̡͡҉͠ ̢̢͘͜͟i̕͜͞ ̡̕d̕o̡͟͝͡n̛͟'̛̀̀ţ̴͟͝ ҉̀̕͟͝w͝a̸͘n̶̷̛͝ţ͘͞ ̶̷̧̀̕ţ̀́ơ̶̸ ҉ḑ̶͜͡͡í̡e̴͝͝ ̷̸̶͡͡ì̡̢͝ ̢͜h͟͏́á̀v҉̢̛ę́͜ ̶̢͘͜͝s͘͟͠͞o̴҉̵ ͞ḿ̶̀́͟ư̵̛͘ć̷́h͞͝͏̨ ̡̛̀͝l͝͏̕͟͢e͘͜͜͝f͜͢t̨̛́͝͡ ̷̨t̨͏̛o̸͘͟͞ ̷s̢̛͟͠͡à͞ỳ̨";
            var message4Regular = "the light.......they said it would be there but it's so faint. is this really the end, i dont want to die please save me i dont want to die i still have so much i wanted to say";

            var message4 = new[]
            {
                "the light",

                "...͝.͘.̵.̷.̢t̵hey ̡sa̡id͝ ́i̛t wou̸ld ͡be ̴t̸h͏ère͢ b̛u͝t̀ ̢i̴t҉'̛s͡ ̢s͞o ̵f͞a͟in͜t. ",


                "ĭ̴͒ͫ̓ͥ͆͊̚sͣ̔ͥͯ҉̀ ̸͛͒͌͘͡t̓̑̒̂́͋̓͑̌͞hͥ͂͐ͦ͑͑i̢ͬsͧ̂̈́̈́͟ ͑͂ȑ̷͗e͌̊̐͜͝ä́̿̅͛ͮͥͩ̚l͑ͣ̋̿́͏l̢̃ͤ͆͒̆̍ͨ̾y̶͑̀ͧ̇̿ͮ ͌ͭ͊ͩ̈́̒̋̚͢͡tͩͪh̸ͬͣ̍̌̿̚͏ĕ̢ͭ̊ͮ̄̌͞ ̷̋͊͑͝eͦ̄̅҉nͨ͑d͗͏̀,̴̴̇̑̒̑ͭ ̔ͪ́̃̋͠i̴̢̅͐̏̚͝ ͯͫ̀d̛̛̀̔͂̊ͭ͐ͬ̾̽͘o̅̑̐̈́̾ͩͨ͟͡n̑ͣ̆̃͗ͥ̚͡t̊̑ͤ̓ͥͤ͛͝ ̛̄̍͛ẃ̨ͨ̊̀͏aͤͮͣͪͫ͋̚n̶ͮ̓̍͢͜ţ̛̎̉ͪ ̈͘͟t̡͗ͯ̃̊̀ǒͤ̆̑ͨͣͦ̽̚ ̨ͣ̿̈̒ͮͫ͐d͒̒̌̋ì͆̅͋ͤ͑͜͢͡e͆̒͌̑ͦ͟ ̉̏͘͟p̔̓͊̋̊̒͠͞ļ̨ͫ͆͗̌̄̀ͩ͏eͩ̈́͋̓͂ͥͣ͝a̸ͧ̄̅ͧ̑̔̆̋ś̓̄eͯ̑̽̑͒̋͟ ̨ͣͯͧͣ͆̇ͧs̷ͫ̍̚a̡͑͠v̓́̐͒̄͌ͫ́ͪe̢̡̓̂̔̓̃ ̀̈̅̐mͤ͋̑̀́͘e̓̔ͦͪ͂́́̕͝ ",




                "i̸͐ͣ̐ͮͭ̊͌̔͒̊́ͤ́̀ ̸̶̡̐͂̈́̈̓͆̂̋ͥͪ̿ͬͤͤ̃̊̚͠͠d͑͒́͊ͥ̓ͣ҉̸̛o̴̓ͨͩͮ͆̎ͮͫ̈́̓ͮ͑̊ͫ̎̉͘͠҉n͂͂ͧ̈̋̇̆̾҉̡̡̕ţ͌͑ͫ̉̂̊ͨ̊̌̑ͯ͒ͦ̄̈͘ ̶̸̉̓́͂͑̈̽̑̍͂ͨ̓̌ͪ̈ͨ͜͝͝w̓ͪ͆ͣ̋ͩͣ̿̎͋̈́̏̈̒҉҉̧̕͡ȧ̇ͯͨ̐̓ͤ͟͟͜͠n̐ͦ̊̔ͮ̒͌ͭͪ̏͏̴̶͘ť̵̛͂͋̈̑ͩ̄́̅͌̆͊͌̎͆ͤͯ̈͘ ̸̨̧͂̏̈͐ͮ̀̽̃̄ͤ̓̓ͣͥͤ̎̏ͩ̿͢ṫ̋̀͐̎ͮ̎̓͆ͮ͂ͬ̓̅ͬ̀̎̕͠ơ̶̧ͧͧ̏̒ͦ̒̀ ̊͒̈́̅̇ͮ͝҉d̵̵̊͑̌ͤ̅̅̒̿̈́͛i̛ͣͥͪ͛̓ͩ͆̽̊ͤ̐ͯ͑̎̾ͧ́̚͜e̷̢̢ͣ̇͌̃̌ͦ̀ͧ͐ͣ͛̊ͣ͐͡ ̷̨̌ͥ͆̓ͦ́̒̂ͯͩ̋̓̀̌̆͋̆̚͞į̧ͨͦ̍̅͋́̄ͣ̄͊͂͋͏ ͌͛̅ͯͥͯ͑͛̈́ͬ̎̈́͠ṡ͗ͣ̅̐ͭ̈́̓̽̍͌̄ͣ̐̃̂̊͐҉̵t̷͒̊́̃͗ͨͯͥ̋ͫͤ̚i̶̶ͪͩͨ̾̋͂̏̌̓ͫ̀ͪ͒͛͂̚͟l̡̛͆ͬ̀͌ͥͮͫ͐ͨ̎ͫ̃ͯͬ͌̅̏̚͜͝ĺ̵ͫͤͨ͛͐̓ͯͨ̍̿̄ͨ̂ͥͥ̎͆̏͜͝͞ ̡̨͑ͩ͐ͫ͋̇̊͌̊̈͋̓̿ͮ̓̚h̴͋̓̐̔ͮͯ͜͟ā̧͑̿̾̽͛̆̉͊̈́ͦ̍ͯ̾̈́ͦ͟͡v̶ͭ̐ͮ̓̌͗̎ͭ͑̑̒̃̍͘e̛ͤ͐̓ͤ̾̀͟͜͡ ̢̢̛̿̃̇̓͛̾ͩ̉̋ͤ̐͑̒̍ͪ̃͋̇͘͡s̸ͨ̅̉ͬ̾͑ͦ̓̓͒́͡͡o͊̉̏ͥ̇ͦ̓ͦ̎̚͏̷҉̕ ̷̔̿̉͒͑͑ͣ́͏mͨ̈̈̋ͫ̄ͩ͠ų̧̃ͫͮ̃́ͤ́͞c̷͐̌͂̂ͦͦ̏̈̈̏̓ͪͨ́͌̂ͯ̈́͋̀͟h̷̶̊̋̀͌̄ͭ̀͗ͣ͛ͥ͢҉͢ ̵̶͋ͮ̎̽ͧ̑̓͐̋̄ͯ͂̌͘͞i̛͆ͩ̅͒ͩ͑͑ͨ͢҉̵͟ ̧̢̿͑̆̃͛ͩ͆̓̒͌̈́ͮ͊̈͋̔̓ͭ͘͠w̷̶̸̡̍͑̆̉̈̓͝a̸̸͊̍̑̅̐̿̽͒͊̀̃̇͛ͬ̒̍͘͡n̶̛̓ͧͬ̊̔͂ͬ͐̐̅́͢͝t̢̨̉̃̊̀̔̓ͨ̆̌̊̽ͧ͂͐ͪ̊̀ḝ́͗̉̊̔̍̊ͤͤ̆̍ͧ̚͞͡d̡ͦ͗͗ͣ͗̏̽͆̍̔͒̎̂ͫ́ ̆ͯͥ̃̿̉̿̓̿ͩͫͯ͛ͭ͘͟͏͜t̄͛͌ͨ̌ͩ̾ͨ͠͞o͛ͣͨͫͧ̂̔͒̄̀̋̓͝҉̸́ ̽̅ͪ̾̿͜͟s̷̢̆̈́͛̿̈́̊̚͢͠͞ā̸̵ͣ̌͒̀̄ͯͧ̇͗͋ͦͬ̈̎͛̆͑́͢͡y̸̷̡͒ͫ͊̊ͤ̽ͣͭ͆̉ͯͬ͝"

            }.ToCSV("");

            chatRoom.PostMessageOrThrow(message4);

            //chatRoom.PostReplyOrThrow(incomingChatMessage, message1);
            //chatRoom.PostMessageOrThrow(message2);

            //Thread.Sleep(8000);

            //chatRoom.PostMessageOrThrow(message3);

            //Thread.Sleep(10000);

            //chatRoom.PostMessageOrThrow(message4);
        }
    }
}
