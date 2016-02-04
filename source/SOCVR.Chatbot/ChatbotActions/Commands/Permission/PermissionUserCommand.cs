using SOCVR.Chatbot.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Permission
{
    internal abstract class PermissionUserCommand : UserCommand
    {
        protected PermissionGroup? MatchInputToPermissionGroup(string userInput)
        {
            //take the input, remove spaces, lower case it
            userInput = userInput
                .Replace(" ", "")
                .ToLower();

            var allPermissionGroups = Enum.GetValues(typeof(PermissionGroup))
                .OfType<PermissionGroup>()
                .Select(x => new
                {
                    EnumVal = x,
                    MatchVal = x.ToString().ToLower()
                });

            var matchingPermissionGroup = allPermissionGroups
                .SingleOrDefault(x => x.MatchVal == userInput);

            return matchingPermissionGroup?.EnumVal;
        }
    }
}
