using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CVChatbot.Model
{
    public static class DBCommonActions
    {
        /// <summary>
        /// Gets the user with the given ID, or creates the user and returns the newly created entry.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static RegisteredUser GetRegisteredUser(int userId, SOChatBotEntities db)
        {
            var existingUser = db.RegisteredUsers.SingleOrDefault(x => x.ChatProfileId == userId);

            if (existingUser != null)
                return existingUser;

            var newUser = new RegisteredUser() { ChatProfileId = userId };
            db.RegisteredUsers.Add(newUser);
            db.SaveChanges();

            return newUser;
        }
    }
}
