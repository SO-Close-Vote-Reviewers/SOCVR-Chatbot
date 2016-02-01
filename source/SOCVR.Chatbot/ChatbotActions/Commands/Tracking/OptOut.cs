using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChatExchangeDotNet;
using SOCVR.Chatbot.Database;

namespace SOCVR.Chatbot.ChatbotActions.Commands.Tracking
{
    internal class OptOut : OptTrackingCommand
    {
        public override string ActionDescription => "Tells the bot to stop tracking your close vote reviewing.";

        public override string ActionName => "Opt Out";

        public override string ActionUsage => "opt out";

        public override PermissionGroup? RequiredPermissionGroup => PermissionGroup.Reviewer;

        protected override string RegexMatchingPattern => "^opt[ -]out$";

        protected override bool GetOptValue() => false;

        protected override string GetPastTencePhrase() => "opted-out";

        protected override string OppositeCommandUsage() => "opt in";

        protected override string TrackingPhrasePrefix() => "of";
    }
}
