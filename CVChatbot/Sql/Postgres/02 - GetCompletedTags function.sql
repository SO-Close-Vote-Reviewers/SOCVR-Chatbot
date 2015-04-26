
create or replace function "GetCompletedTags"(PersonThreshold int, MaxReturnEntries int) returns table
(
	TagName varchar(256),
	PeopleWhoCompletedTag int,
	LastEntryTs timestamptz
) as $$
	with GroupedByTagAndPerson as
	(
		select
			n."TagName",
			n."RegisteredUserId",
			count(n."Id") PersonCount,
			max(n."EntryTs") LastEntryTs
		from "NoItemsInFilterEntry" n
		group by n."TagName", n."RegisteredUserId"
	),
	GroupedByPerson as
	(
		select
			g."TagName",
			count(1) PeopleWhoCompletedTag,
			max(g.LastEntryTs) LastEntryTs
		from GroupedByTagAndPerson g
		group by g."TagName"
		having count(1) >= PersonThreshold
	)
	select
		g."TagName",
		cast(g.PeopleWhoCompletedTag as int),
		g.LastEntryTs
	from GroupedByPerson g
	order by g.LastEntryTs desc
	limit MaxReturnEntries
$$ LANGUAGE sql;