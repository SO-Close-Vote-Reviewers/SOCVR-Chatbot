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





using CVChatbot.Bot;
using System;
using System.Threading;
using TCL.Extensions;

namespace CVChatbot.Console
{
    class Program
    {
        private static RoomManager mng;

        /// <summary>
        /// Waithandle for shutdown.
        /// </summary>
        static ManualResetEvent shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            WriteToConsole("Starting program");

            // Dispose our RoomManager.
            using (mng = new RoomManager())
            {
                mng.ShutdownOrderGiven += mng_ShutdownOrderGiven;
                mng.InformationMessageBroadcasted += mng_InformationMessageBroadcasted;

                WriteToConsole("Gathering settings");

                var settings = new InstallationSettings()
                {
                    ChatRoomUrl = SettingsFileAccessor.GetSettingValue<string>("ChatRoomUrl"),
                    Email = SettingsFileAccessor.GetSettingValue<string>("LoginEmail"),
                    Password = SettingsFileAccessor.GetSettingValue<string>("LoginPassword"),
                    StartUpMessage = SettingsFileAccessor.GetSettingValue<string>("StartUpMessage"),
                    StopMessage = SettingsFileAccessor.GetSettingValue<string>("StopMessage"),
                    MaxReviewLengthHours = SettingsFileAccessor.GetSettingValue<int>("MaxReviewLengthHours"),
                    DefaultCompletedTagsPeopleThreshold = SettingsFileAccessor.GetSettingValue<int>("DefaultCompletedTagsPeopleThreshold"),
                    MaxTagsToFetch = SettingsFileAccessor.GetSettingValue<int>("MaxFetchTags"),
                    DatabaseConnectionString = SettingsFileAccessor.GetSettingValue<string>("DatabaseConnectionString"),
                    PingReviewersDaysBackThreshold = SettingsFileAccessor.GetSettingValue<int>("PingReviewersDaysBackThreshold"),
                    DefaultNextTagCount = SettingsFileAccessor.GetSettingValue<int>("DefaultNextTagCount"),
                };

                // Join the room, but don't post the start up message yet.
                WriteToConsole("Joining room");
                var room = mng.JoinRoom(settings);

                // This could take a few mins, probably best to let
                // the user know we're actually doing something.
                WriteToConsole("Initialising UserWatcherManager");
                using (var watcherManager = new UserWatcherManager(ref room, settings))
                {
                    // Now that we're all set up, post the start up message.
                    mng.PostStartUpMessage(settings);

                    WriteToConsole("Running wait loop");

                    // Wait to get signaled, we do it this way as this is cross-thread.
                    shutdownEvent.WaitOne();
                }
            }
        }

        static void mng_InformationMessageBroadcasted(string message, string author)
        {
            WriteToConsole("[{0}] {1}".FormatInline(author, message));
        }

        static void mng_ShutdownOrderGiven(object sender, EventArgs e)
        {
            WriteToConsole("Shutdown order given.");
            // Signal threads that wait for this.
            shutdownEvent.Set();
        }

        private static object writeToConsoleLockObject = new object();
        private static void WriteToConsole(string message)
        {
            lock (writeToConsoleLockObject)
            {
                // [2000-01-01 00:00:00.00] [<profile id>] (<Message Type>) <message>
                var formattedMessage = "[{0}] {1}".FormatInline(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.ff zzz"), message);
                System.Console.WriteLine(formattedMessage);
            }
        }
    }
}
