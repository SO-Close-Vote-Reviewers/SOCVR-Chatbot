using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tracking
{
    internal class OptIn : OptTrackingCommand
    {
        public override string ActionDescription => "Tells the bot to resume tracking your close vote reviewing.";

        public override string ActionName => "Opt In";

        public override string ActionUsage => "opt in";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        public override bool UserMustBeInAnyPermissionGroupToRun => true;

        protected override string RegexMatchingPattern => "^opt[ -]in$";

        protected override bool GetOptValue() => true;

        protected override string GetPastTencePhrase() => "opted-in";

        protected override string OppositeCommandUsage() => "opt out";

        protected override string TrackingPhrasePrefix() => "to";
    }
}
