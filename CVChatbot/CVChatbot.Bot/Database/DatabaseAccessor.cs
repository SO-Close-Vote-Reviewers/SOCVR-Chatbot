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





using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

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
#if MsSql
            var sql = "insert into RegisteredUser (ChatProfileId, IsOwner) values (@ChatProfileId, 0);";
#elif Postgres
            var sql = "insert into 'RegisteredUser' ('ChatProfileId', 'IsOwner') values (@ChatProfileId, false);".Replace("'", "\"");
#endif

            RunScript(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            });
        }

        public void EditLatestCompletedSessionItemsReviewedCount(int sessionId, int? newItemsReviewedCount)
        {
#if MsSql
            var sql = @"
update ReviewSession
set ItemsReviewed = @NewItemsReviewedCount
where Id = @SessionId;";
#elif Postgres
            var sql = @"
update 'ReviewSession'
set 'ItemsReviewed' = @NewItemsReviewedCount
where 'Id' = @SessionId;".Replace("'", "\"");
#endif

            RunScript(sql, (c) =>
            {
                c.AddParam("@NewItemsReviewedCount", newItemsReviewedCount);
                c.AddParam("@SessionId", sessionId);
            });
        }

        public void EndReviewSession(int sessionId, int? itemsReviewed, DateTimeOffset? endTime = null)
        {
            // Assume the user finished literally just now.
            var timeToInsert = DateTimeOffset.Now;

            if (endTime != null)
            {
                // Use the passed end time rather then assuming the user finish *right now*.
                timeToInsert = endTime.Value;
            }

#if MsSql
            var sql = @"
update ReviewSession
set ItemsReviewed = @ItemsReviewed,
    SessionEnd = SYSDATETIMEOFFSET()
where Id = @SessionId;";
#elif Postgres
            var sql = @"
update 'ReviewSession'
set 'ItemsReviewed' = @ItemsReviewed,
    'SessionEnd' = @SessionEnd
where 'Id' = @SessionId;".Replace("'", "\"");
#endif

            RunScript(sql, (c) =>
            {
                c.AddParam("@ItemsReviewed", itemsReviewed);
                c.AddParam("@SessionId", sessionId);
                c.AddParam("@SessionEnd", timeToInsert);
            });
        }

        public List<CompletedTag> GetCompletedTags(int personThreshold, int maxReturnEntries)
        {
#if MsSql
            var sql = @"select * from GetCompletedTags(@PersonThreshold, @MaxReturnEntries);";
#elif Postgres
            var sql = "select * from 'GetCompletedTags'(@PersonThreshold, @MaxReturnEntries);".Replace("'", "\"");
#endif

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
#if MsSql
                        LastEntryTs = x.Field<DateTimeOffset>("LastEntryTs"),
#elif Postgres
                        LastEntryTs = x.Field<DateTime>("LastEntryTs"),
#endif
                    })
                    .ToList()
            ));
        }

        public DateTimeOffset? GetCurrentSessionStartTs(int chatProfileId)
        {
#if MsSql
            var sql = "select dbo.GetUsersCurrentSession(@ChatProfileId) [SessionStartTs]";
#elif Postgres
            var sql = "select 'GetUsersCurrentSession'(@ChatProfileId) 'SessionStartTs'".Replace("'", "\"");
#endif

            return RunScript<DateTimeOffset?>(sql,
            (c) =>
            {
                c.AddWithValue("@ChatProfileId", chatProfileId);
            },
            new Func<DataRow, DateTimeOffset?>(dr =>
#if MsSql
                dr.Field<DateTimeOffset?>("SessionStartTs")
#elif Postgres
 dr.Field<DateTime?>("SessionStartTs")
#endif
));
        }

        public ReviewSession GetLatestCompletedSession(int chatProfileId)
        {
#if MsSql
            var sql = @"
select top 1 rs.*
from ReviewSession rs
inner join RegisteredUser r on rs.RegisteredUserId = r.Id
where r.ChatProfileId = @ChatProfileId and rs.SessionEnd is not null
order by rs.SessionStart desc";
#elif Postgres
            var sql = @"
select rs.*
from 'ReviewSession' rs
inner join 'RegisteredUser' r on rs.'RegisteredUserId' = r.'Id'
where r.'ChatProfileId' = @ChatProfileId and rs.'SessionEnd' is not null
order by rs.'SessionStart' desc
limit 1;".Replace("'", "\"");
#endif

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
#if MsSql
            var sql = @"
select top 1 rs.*
from ReviewSession rs
inner join RegisteredUser r on rs.RegisteredUserId = r.Id
where r.ChatProfileId = @ChatProfileId and rs.SessionEnd is null
order by rs.SessionStart desc";
#elif Postgres
            var sql = @"
select rs.*
from 'ReviewSession' rs
inner join 'RegisteredUser' r on rs.'RegisteredUserId' = r.'Id'
where r.'ChatProfileId' = @ChatProfileId and rs.'SessionEnd' is null
order by rs.'SessionStart' desc
limit 1;".Replace("'", "\"");
#endif

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
#if MsSql
            var sql = @"
select top 1 rs.*
from ReviewSession rs
inner join RegisteredUser r on rs.RegisteredUserId = r.Id
where r.ChatProfileId = @ChatProfileId
order by rs.SessionStart desc";
#elif Postgres
            var sql = @"
select rs.*
from 'ReviewSession' rs
inner join 'RegisteredUser' r on rs.'RegisteredUserId' = r.'Id'
where r.'ChatProfileId' = @ChatProfileId
order by rs.'SessionStart' desc
limit 1;".Replace("'", "\"");
#endif

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
#if MsSql
            var sql = "select * from GetPingReviewersRecipientList(@MaxDaysBack, @RequestingUserProfileId);";
#elif Postgres
            var sql = "select * from 'GetPingReviewersRecipientList'(@MaxDaysBack, @RequestingUserProfileId);".Replace("'", "\"");
#endif

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
#if MsSql
            var sql = "select * from RegisteredUser where ChatProfileId = @ChatProfileId;";
#elif Postgres
            var sql = "select * from 'RegisteredUser' where 'ChatProfileId' = :ChatProfileId".Replace("'", "\"");
#endif

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
#if MsSql
            var sql = "select * from GetUserAuditStats(@ChatProfileId) a order by a.[Percent] desc;";
#elif Postgres
            var sql = "select * from 'GetUserAuditStats'(@ChatProfileId) a order by a.Percent desc;".Replace("'", "\"");
#endif

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
#if MsSql
            var sql = "select * from GetUserCompletedTags(@ChatProfileId) order by LastTimeCleared desc;";
#elif Postgres
            var sql = "select * from 'GetUserCompletedTags'(@ChatProfileId) order by LastTimeCleared desc;".Replace("'", "\"");
#endif

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
#if MsSql
                        LastTimeCleared = x.Field<DateTimeOffset>("LastTimeCleared")
#elif Postgres
                        LastTimeCleared = x.Field<DateTime>("LastTimeCleared")
#endif
                    })
                    .ToList()
            ));
        }

        public void InsertCompletedAuditEntry(int chatProfileId, string tagName)
        {
#if MsSql
            var sql = @"
insert into CompletedAuditEntry(RegisteredUserId, TagName, EntryTs)
	select
		ru.Id,
		@TagName,
		SYSDATETIMEOFFSET()
	from RegisteredUser ru
	where ru.ChatProfileId = @ChatProfileId;";
#elif Postgres
            var sql = @"
insert into 'CompletedAuditEntry'('RegisteredUserId', 'TagName', 'EntryTs')
	select
		ru.'Id',
		@TagName,
		current_timestamp
	from 'RegisteredUser' ru
	where ru.'ChatProfileId' = @ChatProfileId;".Replace("'", "\"");
#endif

            RunScript(sql, (c) =>
            {
                c.AddParam("@TagName", tagName);
                c.AddParam("@ChatProfileId", chatProfileId);
            });
        }

        public void InsertNoItemsInFilterRecord(int chatProfileId, string tagName)
        {
#if MsSql
            var sql = @"
insert into NoItemsInFilterEntry(RegisteredUserId, TagName, EntryTs)
	select
		ru.Id,
		@TagName,
		SYSDATETIMEOFFSET()
	from RegisteredUser ru
	where ru.ChatProfileId = @ChatProfileId;";
#elif Postgres
            var sql = @"
insert into 'NoItemsInFilterEntry'('RegisteredUserId', 'TagName', 'EntryTs')
	select
		ru.'Id',
		@TagName,
		current_timestamp
	from 'RegisteredUser' ru
	where ru.'ChatProfileId' = @ChatProfileId;".Replace("'", "\"");
#endif

            RunScript(sql, (c) =>
            {
                c.AddParam("@TagName", tagName);
                c.AddParam("@ChatProfileId", chatProfileId);
            });
        }

        public void InsertUnrecognizedCommand(string unrecognizedCommand)
        {
#if MsSql
            var sql = "insert into UnrecognizedCommand([Command]) values (@Command);";
#elif Postgres
            var sql = "insert into 'UnrecognizedCommand'('Command') values (@Command);".Replace("'", "\"");
#endif

            RunScript(sql, (c) => c.AddParam("@Command", unrecognizedCommand));
        }

        public void SetSessionEndTs(int sessionId, DateTimeOffset newSessionEndTs)
        {
#if MsSql
            var sql = @"
update ReviewSession
set SessionEnd = @NewSessionEndTs
where Id = @SessionId;";
#elif Postgres
            var sql = @"
update 'ReviewSession'
set 'SessionEnd' = @NewSessionEndTs
where 'Id' = @SessionId;".Replace("'", "\"");
#endif

            RunScript(sql, (c) =>
            {
                c.AddWithValue("@NewSessionEndTs", newSessionEndTs);
                c.AddWithValue("@SessionId", sessionId);
            });
        }

        public void StartReviewSession(int chatProfileId)
        {
#if MsSql
            var sql = @"
insert into ReviewSession (RegisteredUserId, SessionStart)
	select
		ru.Id,
		SYSDATETIMEOFFSET()
	from RegisteredUser ru
	where ru.ChatProfileId = @ChatProfileId;";
#elif Postgres
            var sql = @"
insert into 'ReviewSession' ('RegisteredUserId', 'SessionStart')
	select
		ru.'Id',
		@StartTime
	from 'RegisteredUser' ru
	where ru.'ChatProfileId' = @ChatProfileId;".Replace("'", "\"");
#endif

            RunScript(sql, (c) =>
            {
                c.AddParam("@ChatProfileId", chatProfileId);
#if Postgres
                c.AddParam("@StartTime", DateTimeOffset.Now);
#endif
            });
        }

        private ReviewSession ConvertDataRowToReviewSession(DataRow dr)
        {
            var rs = new ReviewSession();

            rs.Id = dr.Field<int>("Id");
            rs.RegisteredUserId = dr.Field<int>("RegisteredUserId");
#if MsSql
            rs.SessionStart = dr.Field<DateTimeOffset>("SessionStart");
            rs.SessionEnd = dr.Field<DateTimeOffset?>("SessionEnd");
#elif Postgres
            rs.SessionStart = dr.Field<DateTime>("SessionStart");
            rs.SessionEnd = dr.Field<DateTime?>("SessionEnd");
#endif
            rs.ItemsReviewed = dr.Field<int?>("ItemsReviewed");

            return rs;
        }

        public int EndAnyOpenSessions(int profileId)
        {
#if MsSql
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
#elif Postgres
            var sql = @"
update 'ReviewSession'
set 'SessionEnd' = current_timestamp
from 'ReviewSession' rs
inner join 'RegisteredUser' ru on rs.'RegisteredUserId' = ru.'Id'
where
	ru.'ChatProfileId' = @ChatProfileId and
	rs.'SessionEnd' is null
returning rs.'Id';".Replace("'", "\"");
#endif

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
