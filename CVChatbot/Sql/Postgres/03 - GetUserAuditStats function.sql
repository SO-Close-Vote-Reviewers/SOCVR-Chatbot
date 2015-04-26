
create or replace function "GetUserAuditStats"(ChatProfileId int) returns table
(
	TagName varchar(256),
	Percent decimal,
	"Count" int
) LANGUAGE plpgsql as $$
declare
	totalAuditCount int;
begin
	totalAuditCount :=( 
		select count(1)
		from "CompletedAuditEntry" a
		inner join "RegisteredUser" r on a."RegisteredUserId" = r."Id"
		where r."ChatProfileId" = ChatProfileId);

	return query
	with GroupedData as
	(
		select
			a."TagName",
			cast(count(1) as int) TagCount
		from "CompletedAuditEntry" a
		inner join "RegisteredUser" r on a."RegisteredUserId" = r."Id"
		where r."ChatProfileId" = ChatProfileId
		group by a."TagName"
	),
	PercentData as
	(
		select
			gd."TagName",
			gd.TagCount,
			(gd.TagCount * 1.0 / totalAuditCount * 100) Percent
		from GroupedData gd
	)
	select
		pd."TagName",
		pd.Percent,
		pd.TagCount
	from PercentData pd;
end
$$;
