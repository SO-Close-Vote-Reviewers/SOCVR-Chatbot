using System;
using System.Threading;
using TCL.Extensions;
using SOCVR.Chatbot.ChatRoom;
using SOCVR.Chatbot.Database;
using Microsoft.Data.Entity;
using System.Linq;
using System.Collections.Generic;

namespace SOCVR.Chatbot
{
    class Program
    {
        private static RoomManager mng;

        /// <summary>
        /// waithandle for shutdown
        /// </summary>
        static ManualResetEvent shutdownEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            WriteToConsole("Starting program");

            using (var db = new DatabaseContext())
            {
                //create the database if it does not exist and push
                //and new migrations to it
                db.Database.Migrate();

                //yes, yes I'm really doing this
                var roList = new[] { 1043380, 2246344 };

                foreach (var roId in roList)
                {
                    if (!db.Users.Any(x => x.ProfileId == roId))
                    {
                        db.Users.Add(new User
                        {
                            ProfileId = roId,
                            Permissions = new List<UserPermission>
                            {
                                new UserPermission { PermissionGroup = PermissionGroup.Reviewer },
                                new UserPermission { PermissionGroup = PermissionGroup.BotOwner }
                            }
                        });
                        db.SaveChanges();
                    }
                }
            }

            // dispose our RoomManager
            using (mng = new RoomManager())
            {
                mng.ShutdownOrderGiven += mng_ShutdownOrderGiven;
                mng.InformationMessageBroadcasted += mng_InformationMessageBroadcasted;

                WriteToConsole("Gathering settings");

                WriteToConsole("Joining room");
                mng.JoinRoom();

                WriteToConsole("Running wait loop");

                // wait to get signaled
                // we do it this way because this is cross-thread
                shutdownEvent.WaitOne();
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
