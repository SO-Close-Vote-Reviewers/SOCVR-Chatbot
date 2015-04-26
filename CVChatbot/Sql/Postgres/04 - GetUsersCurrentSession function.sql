
create or replace function "GetUserAuditStats"(ChatProfileId int) returns timestamptz LANGUAGE plpgsql as $$
declare
	returnValue int = null;
begin
	select
		rs."SessionStart" into returnValue
	from "ReviewSession" rs
	inner join "RegisteredUser" r on rs."RegisteredUserId" = r."Id"
	where
		r."ChatProfileId" = ChatProfileId and
		rs."SessionEnd" is null
	order by rs."SessionStart" desc
	limit 1;

	return returnValue;
end
$$;