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

namespace CVChatbot.Bot
{
    /// <summary>
    /// Helper to throw exceptions to enforce incoming contracts.
    /// </summary>
    public static class ThrowWhen
    {
        /// <summary>
        /// Throws an argument exception if value is null or empty.
        /// </summary>
        /// <param name="value">A value.</param>
        /// <param name="paramName">Friendly name of value.</param>
        public static void IsNullOrEmpty(string value, string paramName)
        {
            if (String.IsNullOrEmpty(value))
            {
                throw new ArgumentException(String.Format("'{0}' must not be null or empty.", paramName), paramName);
            }
        }
    }
}
