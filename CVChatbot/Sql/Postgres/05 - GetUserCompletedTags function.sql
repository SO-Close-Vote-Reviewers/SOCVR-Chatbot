
create or replace function "GetUserCompletedTags"(ChatProfileId int) returns table
(
	TagName varchar(256),
	TimesCleared int,
	LastTimeCleared timestamptz
) as $$
	with DataPool as
	(
		select
			i."TagName",
			i."EntryTs"
		from "NoItemsInFilterEntry" i
		inner join "RegisteredUser" ru on i."RegisteredUserId" = ru."Id"
		where
			ru."ChatProfileId" = ChatProfileId
	)
	select
		dp."TagName",
		cast(count(1) as int) TimesCleared,
		max(dp."EntryTs") LastTimeCleared
	from DataPool dp
	group by dp."TagName"
$$ LANGUAGE sql;