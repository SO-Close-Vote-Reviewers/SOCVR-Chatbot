/*
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





using System;
using System.Collections.Generic;
using ChatExchangeDotNet;
using CVChatbot.Bot;

namespace CVChatbot
{
    /// <summary>
    /// Creates a singleton SedeClient for getting the tag listing from SEDE.
    /// </summary>
    public static class SedeAccessor
    {
        /// <summary>
        /// The last url with revision.
        /// </summary>
        private static string lastRevision;

        /// <summary>
        /// The last time fresh tag data was fetched.
        /// </summary>
        private static DateTime lastRevIdCheckTime;

        private static string loginEmail;

        private static string loginPassword;

        // We have once client session wide.
        private static SedeClient sedeClient = null;

        // We run the query and keep the result for a while.
        private static Dictionary<string, int> tags = null;

        /// <summary>
        /// A singleton for holing the SEDE client.
        /// </summary>
        private static SedeClient Client
        {
            get
            {
                if (sedeClient == null)
                {
                    sedeClient = new SedeClient(loginEmail, loginPassword);
                }
                return sedeClient;
            }
        }

        /// <summary>
        /// Sets the time that the tag data was pulled at to a long time ago.
        /// This will ensure that the next call to GetTags will get fresh data.
        /// </summary>
        public static void InvalidateCache()
        {
            lastRevIdCheckTime = DateTime.MinValue;
        }

        /// <summary>
        /// Gets the tags from the SEDE query. If the email and password has not already
        /// been set then those credentials will be saved and used. Will also tell
        /// the chat room if the tags are being refreshed (because it takes some time).
        /// </summary>
        /// <param name="chatRoom"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetTags(Room chatRoom, string email, string password)
        {
            // Set the email/password if not set.
            if (loginEmail == null)
            {
                loginEmail = email;
                loginPassword = password;
            }

            // If tags have not been gotten yet or its been more than 30 minutes since the last get
            // then refresh the tag listing.
            if (tags == null || (DateTime.UtcNow - lastRevIdCheckTime).TotalMinutes > 30)
            {
                chatRoom.PostMessageOrThrow("Refreshing the tag listing. Please wait...");

                var currentID = "";

                if ((currentID = Client.GetSedeQueryRunUrl(236526)) != lastRevision || tags == null)
                {
                    lastRevision = currentID;
                    tags = Client.GetTags(lastRevision);
                }

                lastRevIdCheckTime = DateTime.UtcNow;
            }
            return tags;
        }
    }
}