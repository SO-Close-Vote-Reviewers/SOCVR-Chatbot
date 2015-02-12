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

        /// <summary>
        /// Returns the registered user that contains the given chat profile id.
        /// Returns null if the user cannot be found.
        /// </summary>
        /// <param name="chatProfileId"></param>
        /// <returns></returns>
        public RegisteredUser GetRegisteredUserByChatProfileId(int chatProfileId)
        {
            var sql = "select * from RegisteredUser where ChatProfileId = @ChatProfileId;";

            return RunScript<RegisteredUser>(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            },
            new Func<DataTable, RegisteredUser>(dt =>
                dt.AsEnumerable()
                    .Select(x => new RegisteredUser
                    {
                        Id = x.Field<int>("Id"),
                        ChatProfileId = x.Field<int>("ChatProfileId"),
                        IsOwner = x.Field<bool>("IsOwner")
                    })
                    .SingleOrDefault()
            ));
        }

        public void AddUserToRegisteredUsersList(int chatProfileId)
        {
            var sql = "insert into RegisteredUser (ChatProfileId, IsOwner) values (@ChatProfileId, 0);";

            RunScript(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            });
        }

        public List<UserAuditStatEntry> GetUserAuditStats(int chatProfileId)
        {
            var sql = "select * from GetUserAuditStats(@ChatProfileId) a order by a.[Percent] desc;";

            return RunScript<List<UserAuditStatEntry>>(sql,
            (c) =>
            {
                c.AddWithValue(@"ChatProfileId", chatProfileId);
            },
            new Func<DataTable, List<UserAuditStatEntry>>(dt =>
                dt.AsEnumerable()
                    .Select(x => new UserAuditStatEntry
                    {
                        TagName = x.Field<string>("TagName"),
                        Percent = x.Field<decimal>("Percent"),
                        Count = x.Field<int>("Count")
                    })
                    .ToList()
            ));
        }

        public void StartReviewSession(int chatProfileId)
        {
            var sql = @"
insert into ReviewSession (RegisteredUserId, SessionStart)
	select
		ru.Id,
		SYSDATETIMEOFFSET()
	from RegisteredUser ru
	where ru.ChatProfileId = @ChatProfileId;";

            RunScript(sql, (c) =>
            {
                c.AddParam("@ChatProfileId", chatProfileId);
            });
        }

        public DateTimeOffset? GetCurrentSessionStartTs(int chatProfileId)
        {
            var sql = "select dbo.GetUsersCurrentSession(@ChatProfileId) [SessionStartTs]";

            return RunScript<DateTimeOffset?>(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            },
            new Func<DataRow, DateTimeOffset?>(dr =>
                dr.Field<DateTimeOffset?>("SessionStartTs")
            ));
        }

        public ReviewSession GetLatestSessionForUser(int chatProfileId)
        {
            var sql = @"
select top 1 rs.*
from ReviewSession rs
inner join RegisteredUser r on rs.RegisteredUserId = r.Id
where r.ChatProfileId = @ChatProfileId
order by rs.SessionStart desc";

            return RunScript<ReviewSession>(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            },
            new Func<DataTable, ReviewSession>(dt =>
                dt.AsEnumerable()
                    .Select(ConvertDataRowToReviewSession)
                    .SingleOrDefault()
            ));
        }

        public void EditLatestCompletedSessionItemsReviewedCount(int sessionId, int? newItemsReviewedCount)
        {
            var sql = @"
update ReviewSession
set ItemsReviewed = @NewItemsReviewedCount
where Id = @SessionId;";

            RunScript(sql, (c) =>
            {
                c.AddParam("@NewItemsReviewedCount", newItemsReviewedCount);
            });
        }

        public ReviewSession GetLatestCompletedSession(int chatProfileId)
        {
            var sql = @"
select top 1 rs.*
from ReviewSession rs
inner join RegisteredUser r on rs.RegisteredUserId = r.Id
where r.ChatProfileId = @ChatProfileId and rs.SessionEnd is not null
order by rs.SessionStart desc";

            return RunScript<ReviewSession>(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            },
            new Func<DataTable, ReviewSession>(dt =>
                dt.AsEnumerable()
                    .Select(ConvertDataRowToReviewSession)
                    .SingleOrDefault()
            ));
        }

        private ReviewSession ConvertDataRowToReviewSession(DataRow dr)
        {
            return new ReviewSession
            {
                Id = dr.Field<int>("Id"),
                RegisteredUserId = dr.Field<int>("RegisteredUserId"),
                SessionStart = dr.Field<DateTimeOffset>("SessionStart"),
                SessionEnd = dr.Field<DateTimeOffset?>("SessionEnd"),
                ItemsReviewed = dr.Field<int?>("ItemsReviewed"),
            };
        }

        public void SetSessionEndTs(int sessionId, DateTimeOffset newSessionEndTs)
        {
            var sql = @"
update ReviewSession
set SessionEnd = @NewSessionEndTs
where Id = @SessionId;";

            RunScript(sql, (c) =>
            {
                c.AddWithValue("@NewSessionEndTs", newSessionEndTs);
                c.AddWithValue("@SessionId", sessionId);
            });
        }
    }
}
