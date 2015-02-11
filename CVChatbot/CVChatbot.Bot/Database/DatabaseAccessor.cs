using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheCommonLibrary.Sql;
using System.Data;
using TheCommonLibrary.Extensions;

namespace CVChatbot.Bot.Database
{
    class DatabaseAccessor : SqlScriptAccessorBase
    {
        public DatabaseAccessor(string connectionString) : base(connectionString) { }

        public List<CompletedTag> GetCompletedTags(int personThreshold, int maxReturnEntries)
        {
            var sql = @"select * from GetCompletedTags(@PersonThreshold, @MaxReturnEntries);";

            return RunScript<List<CompletedTag>>(sql,
                (c) =>
                {
                    c.AddWithValue("@PersonThreshold", personThreshold);
                    c.AddWithValue("@MaxReturnEntries", maxReturnEntries);
                },
                new Func<DataTable, List<CompletedTag>>(dt =>
                    dt.AsEnumerable()
                        .Select(x => new CompletedTag
                        {
                            TagName = x.Field<string>("TagName"),
                            PeopleWhoCompletedTag = x.Field<int>("PeopleWhoCompletedTag"),
                            LastEntryTs = x.Field<DateTimeOffset>("LastEntryTs"),
                        })
                        .ToList()
                ));
        }


    }
}
