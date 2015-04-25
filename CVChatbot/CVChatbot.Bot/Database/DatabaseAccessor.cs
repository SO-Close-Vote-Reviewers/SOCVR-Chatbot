using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCL.DataAccess;
using System.Data;
using TCL.Extensions;

#if MsSql
using TCL.DataAccess.MsSql;
#elif Postgres
using TCL.DataAccess.Postgres;
#endif

namespace CVChatbot.Bot.Database
{
    class DatabaseAccessor : 
#if MsSql
        SqlScriptAccessorBase
#elif Postgres
        PostgresScriptAccessorBase
#endif
    {
        public DatabaseAccessor(string connectionString) : base(connectionString) { }

        public void AddUserToRegisteredUsersList(int chatProfileId)
        {
            var sql = "insert into RegisteredUser (ChatProfileId, IsOwner) values (@ChatProfileId, 0);";

            RunScript(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            });
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
                c.AddParam("@SessionId", sessionId);
            });
        }

        public void EndReviewSession(int sessionId, int? itemsReviewed)
        {
            var sql = @"
update ReviewSession
set ItemsReviewed = @ItemsReviewed,
    SessionEnd = SYSDATETIMEOFFSET()
where Id = @SessionId;";

            RunScript(sql, (c) =>
            {
                c.AddParam("@ItemsReviewed", itemsReviewed);
                c.AddParam("@SessionId", sessionId);
            });
        }

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

        public ReviewSession GetLatestOpenSessionForUser(int chatProfileId)
        {
            var sql = @"
select top 1 rs.*
from ReviewSession rs
inner join RegisteredUser r on rs.RegisteredUserId = r.Id
where r.ChatProfileId = @ChatProfileId and rs.SessionEnd is null
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

        public List<int> GetPingReviewersRecipientList(int requestingUserProfileId, int daysBackThreshold)
        {
            var sql = "select * from GetPingReviewersRecipientList(@MaxDaysBack, @RequestingUserProfileId);";

            return RunScript<List<int>>(sql,
            (c) =>
            {
                c.AddParam("@MaxDaysBack", daysBackThreshold);
                c.AddParam("@RequestingUserProfileId", requestingUserProfileId);
            },
            new Func<DataTable, List<int>>(dt =>
                dt.AsEnumerable()
                    .Select(x => x.Field<int>("ChatProfileId"))
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

        public List<UserCompletedTag> GetUserCompletedTags(int chatProfileId)
        {
            var sql = "select * from GetUserCompletedTags(@ChatProfileId) order by LastTimeCleared desc;";

            return RunScript<List<UserCompletedTag>>(sql,
            (c) =>
            {
                c.AddParam("@ChatProfileId", chatProfileId);
            },
            new Func<DataTable, List<UserCompletedTag>>(dt =>
                dt.AsEnumerable()
                    .Select(x => new UserCompletedTag
                    {
                        TagName = x.Field<string>("TagName"),
                        TimesCleared = x.Field<int>("TimesCleared"),
                        LastTimeCleared = x.Field<DateTimeOffset>("LastTimeCleared")
                    })
                    .ToList()
            ));
        }

        public void InsertCompletedAuditEntry(int chatProfileId, string tagName)
        {
            var sql = @"
insert into CompletedAuditEntry(RegisteredUserId, TagName, EntryTs)
	select
		ru.Id,
		@TagName,
		SYSDATETIMEOFFSET()
	from RegisteredUser ru
	where ru.ChatProfileId = @ChatProfileId;";

            RunScript(sql, (c) =>
            {
                c.AddParam("@TagName", tagName);
                c.AddParam("@ChatProfileId", chatProfileId);
            });
        }

        public void InsertNoItemsInFilterRecord(int chatProfileId, string tagName)
        {
            var sql = @"
insert into NoItemsInFilterEntry(RegisteredUserId, TagName, EntryTs)
	select
		ru.Id,
		@TagName,
		SYSDATETIMEOFFSET()
	from RegisteredUser ru
	where ru.ChatProfileId = @ChatProfileId;";

            RunScript(sql, (c) =>
            {
                c.AddParam("@TagName", tagName);
                c.AddParam("@ChatProfileId", chatProfileId);
            });
        }

        public void InsertUnrecognizedCommand(string unrecognizedCommand)
        {
            var sql = "insert into UnrecognizedCommand([Command]) values (@Command);";

            RunScript(sql, (c) => c.AddParam("@Command", unrecognizedCommand));
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

        public int EndAnyOpenSessions(int profileId)
        {
            var sql = @"
update rs
set rs.SessionEnd = dateadd(SECOND, 40, rs.SessionStart)
output
	inserted.Id
from ReviewSession rs
inner join RegisteredUser ru on rs.RegisteredUserId = ru.Id
where
	ru.ChatProfileId = @ChatProfileId and
	rs.SessionEnd is null;";

            var numOfSessionsClosed = RunScript<int>(sql,
            (c) =>
            {
                c.AddParam("@ChatProfileId", profileId);
            },
            new Func<DataTable, int>(dt =>
            {
                return dt.AsEnumerable()
                    .Select(x => x.Field<int>("Id"))
                    .Count();
            }));

            return numOfSessionsClosed;
        }
    }
}
