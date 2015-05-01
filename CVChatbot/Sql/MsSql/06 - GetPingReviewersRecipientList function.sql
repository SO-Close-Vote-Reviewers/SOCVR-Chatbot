SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION GetPingReviewersRecipientList 
(
	@MaxDaysBack int,
	@ChatProfileIdRequestingInform int
)
RETURNS 
@Return TABLE 
(
	ChatProfileId int not null
)
AS
BEGIN
	
	insert into @Return(ChatProfileId)
		select
			ru.ChatProfileId
		from ReviewSession rs
		inner join RegisteredUser ru on rs.RegisteredUserId = ru.Id
		where
			ru.ChatProfileId != @ChatProfileIdRequestingInform and
			rs.SessionStart is not null and
			rs.SessionEnd > dateadd(day, -@MaxDaysBack, SYSDATETIMEOFFSET())
		group by
			ru.ChatProfileId
	
	RETURN 
END
GO