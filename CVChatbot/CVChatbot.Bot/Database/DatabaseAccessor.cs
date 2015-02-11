using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Sql;
using System.Data;

namespace CVChatbot.Bot.Database
{
    class DatabaseAccessor : SqlScriptAccessorBase
    {
        public DatabaseAccessor(string connectionString) : base(connectionString) { }

        public List<object> GetCompletedTags(int personThreshold, int maxReturnEntries)
        {
            throw new NotImplementedException();
        }


    }
}
