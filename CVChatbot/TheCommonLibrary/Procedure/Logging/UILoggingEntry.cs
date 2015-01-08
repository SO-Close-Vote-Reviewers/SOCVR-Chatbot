using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCommonLibrary.Procedure.Logging
{
    internal class UILoggingEntry
    {
        public string Message { get; set; }
        public UILoggingEntryType EntryType { get; set; }
    }

    internal static class ItemStatusEntryColorMappings
    {
        public static Dictionary<UILoggingEntryType, FrontAndBackColor> ColorMapping = new Dictionary<UILoggingEntryType, FrontAndBackColor>()
        {
            { UILoggingEntryType.Information, new FrontAndBackColor{FrontColor = Color.Black, BackColor = Color.White} },
            { UILoggingEntryType.Success, new FrontAndBackColor{ FrontColor = Color.Black, BackColor = Color.LimeGreen} },
            { UILoggingEntryType.Warning, new FrontAndBackColor{ FrontColor = Color.Black, BackColor = Color.Yellow} },
            { UILoggingEntryType.Error, new FrontAndBackColor{ FrontColor = Color.White, BackColor = Color.DarkRed} },
            { UILoggingEntryType.Header, new FrontAndBackColor{ FrontColor = Color.White, BackColor = Color.DarkGray} },
        };
    }

    internal class FrontAndBackColor
    {
        public Color BackColor { get; set; }
        public Color FrontColor { get; set; }
    }

    public enum UILoggingEntryType
    {
        Information,
        Success,
        Warning,
        Error,
        Header
    }
}
