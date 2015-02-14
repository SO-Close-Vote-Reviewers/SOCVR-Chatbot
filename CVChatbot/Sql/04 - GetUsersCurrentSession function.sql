
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create FUNCTION GetUsersCurrentSession
(
	@ChatProfileId int
)
RETURNS datetimeoffset
AS
BEGIN
	declare @returnValue datetimeoffset = null;

	select top 1
		@returnValue = rs.SessionStart
	from ReviewSession rs
	inner join RegisteredUser r on rs.RegisteredUserId = r.Id
	where
		r.ChatProfileId = @ChatProfileId and
		rs.SessionEnd is null
	order by rs.SessionStart desc
	
	RETURN @returnValue
END
GO
