using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using Microsoft.Data.Entity;
using SOCVR.Chatbot.ChatRoom;
using SOCVR.Chatbot.Database;
using TCL.Extensions;

namespace SOCVR.Chatbot
{
    static class Program
    {
        private static RoomManager mng;
#pragma warning disable 0414
        private static UserTracking watcher; // Accessed by reflection.
#pragma warning restore 0414

        /// <summary>
        /// wait handle for shutdown
        /// </summary>
        static ManualResetEvent shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            WriteToConsole("Starting program");

            // dispose our RoomManager
            using (mng = new RoomManager())
            {
                mng.ShutdownOrderGiven += mng_ShutdownOrderGiven;
                mng.InformationMessageBroadcasted += mng_InformationMessageBroadcasted;

                WriteToConsole("Joining room");
                mng.JoinRoom();

                InitializeDatbase();

                WriteToConsole("Connecting chat event listeners");
                mng.ConnectEventDelegates();

                WriteToConsole("Starting user tracker");
                var rm = mng.CvChatRoom;
                using (watcher = new UserTracking(ref rm))
                {
                    mng.PostStartupMessage();

                    WriteToConsole("Running wait loop");

                    // wait to get signaled
                    // we do it this way because this is cross-thread
                    shutdownEvent.WaitOne();
                }
            }
        }

        private static void InitializeDatbase()
        {
            using (var db = new DatabaseContext())
            {
                WriteToConsole("Connecting to database");

                bool dbSetUp = false;

                //loop until the connection works
                while (!dbSetUp)
                {
                    try
                    {
                        //create the database if it does not exist and push and new migrations to it
                        db.Database.Migrate();
                        dbSetUp = true;
                        WriteToConsole("Connected to database");
                    }
                    catch (SocketException ex)
                    {
                        WriteToConsole("Caught error when trying to set up database. Waiting 30 seconds to retry.");
                        WriteToConsole(ex.Message);
                        Thread.Sleep(30 * 1000);
                    }
                }

                // We only need to check the RO list on the bot's first start up.
                if (!db.UserPermissions.Any(x => x.PermissionGroup == PermissionGroup.BotOwner))
                {
                    EnsureRoomOwnersAreInDatabase();
                }
            }
        }

        private static void EnsureRoomOwnersAreInDatabase()
        {
            // We'll need to wait for the room's user
            // lists to be populated (since we init it async)
            // before adding the ROs to the DB.
            while (mng.CvChatRoom.RoomOwners.Count == 0)
            {
                Thread.Sleep(1000);
            }

            using (var db = new DatabaseContext())
            {
                foreach (var ro in mng.CvChatRoom.RoomOwners)
                {
                    db.EnsureUserExists(ro.ID, true);
                    db.EnsureUserIsInAllPermissionGroups(ro.ID);
                }
            }
        }

        static void mng_InformationMessageBroadcasted(string message, string author)
        {
            WriteToConsole($"[{author}] {message}");
        }

        static void mng_ShutdownOrderGiven(object sender, EventArgs e)
        {
            WriteToConsole("Shutdown order given.");
            // signal threads that wait for this
            shutdownEvent.Set();
        }

        private static object writeToConsoleLockObject = new object();
        public static void WriteToConsole(string message)
        {
            lock (writeToConsoleLockObject)
            {
                // [2000-01-01 00:00:00.00] [<profile id>] (<Message Type>) <message>
                var formattedMessage = "[{0}] {1}".FormatInline(DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.ff zzz"), message);
                Console.WriteLine(formattedMessage);
            }
        }
    }
}
