using BrightIdeasSoftware;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Extensions;

namespace TheCommonLibrary.Procedure.Logging
{
    /// <summary>
    /// Class responsible for logging information to the UI and log files
    /// </summary>
    public class LoggingManager
    {
        private List<UILoggingEntry> uiLoggingEntries;
        private ObjectListView reportingOLV;
        private string logFileName;
        private int linesWritenToLogFile;

        public LoggingManager(ObjectListView olvToReportTo)
        {
            reportingOLV = olvToReportTo;
            reportingOLV.FormatRow += reportingOLV_FormatRow;

            logFileName = "ProcedureLog_" + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss.ffff") + ".log";

            if (!Directory.Exists("Logs"))
            {
                Directory.CreateDirectory("Logs");
            }

            ClearList();
        }

        private void reportingOLV_FormatRow(object sender, FormatRowEventArgs e)
        {
            UILoggingEntry entry = (UILoggingEntry)e.Model;

            var colorData = ItemStatusEntryColorMappings.ColorMapping[entry.EntryType];

            e.Item.BackColor = colorData.BackColor;
            e.Item.ForeColor = colorData.FrontColor;
        }

        public void ClearList()
        {
            uiLoggingEntries = new List<UILoggingEntry>();

            RefreshOLV();
        }

        public void AddUILogMessage(string message, UILoggingEntryType entryType, bool copyToFileLogging)
        {
            uiLoggingEntries.Add(new UILoggingEntry()
            {
                EntryType = entryType,
                Message = message
            });

            if (copyToFileLogging)
                AddFileLogMessage(message);

            RefreshOLV();
        }

        public void AddUILogMessage(string message, UILoggingEntryType entryType)
        {
            AddUILogMessage(message, entryType, false);
        }

        public void AddFileLogMessage(string message)
        {
            var logFilePath = Path.Combine("Logs", logFileName);
            var formattedMessage = MakeFormatedLineForFileOutput(message);
            File.AppendAllText(logFilePath, formattedMessage + Environment.NewLine);
        }

        private string MakeFormatedLineForFileOutput(string message)
        {
            // [yyyy-MM-dd HH:mm:ss.ff][###] message

            var dateTimeString = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ff");
            var messageHeader = "[{0}][{1}] ".FormatInline(dateTimeString, linesWritenToLogFile);

            linesWritenToLogFile += 1;

            var splitMessage = message.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);

            var formattedMessage = splitMessage
                .Select(x => messageHeader + x)
                .ToCSV(Environment.NewLine);

            return formattedMessage;
        }

        private void RefreshOLV()
        {
            reportingOLV.Objects = uiLoggingEntries;
        }
    }
}
