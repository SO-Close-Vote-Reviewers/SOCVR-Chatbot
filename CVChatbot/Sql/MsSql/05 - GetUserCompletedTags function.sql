SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION GetUserCompletedTags
(
	@ChatProfileId int
)
RETURNS 
@Return TABLE 
(
	TagName nvarchar(max) not null,
	TimesCleared int not null,
	LastTimeCleared datetimeoffset not null
)
AS
BEGIN
	
	;with DataPool as
	(
		select
			i.TagName,
			i.EntryTs
		from NoItemsInFilterEntry i
		inner join RegisteredUser ru on i.RegisteredUserId = ru.Id
		where
			ru.ChatProfileId = @ChatProfileId
	)
	insert into @Return (TagName, TimesCleared, LastTimeCleared)
		select
			dp.TagName,
			count(1) [TimesCleared],
			max(dp.EntryTs) [LastTimeCleared]
		from DataPool dp
		group by
			dp.TagName
	
	RETURN 
END
GO