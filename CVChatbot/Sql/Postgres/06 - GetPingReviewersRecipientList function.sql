
create or replace function "GetPingReviewersRecipientList"(MaxDaysBack int, ChatProfileIdRequestingInform int) returns table
(
	ChatProfileId int
) as $$
	select
		ru."ChatProfileId"
	from "ReviewSession" rs
	inner join "RegisteredUser" ru on rs."RegisteredUserId" = ru."Id"
	where
		ru."ChatProfileId" != ChatProfileIdRequestingInform and
		rs."SessionStart" is not null and
		rs."SessionEnd" > (current_timestamp - (MaxDaysBack || ' days')::interval)
	group by
		ru."ChatProfileId"
$$ LANGUAGE sql;