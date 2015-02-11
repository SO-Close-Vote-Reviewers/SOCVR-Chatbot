SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create FUNCTION GetCompletedTags
(
	@PersonThreshold int,
	@MaxReturnEntries int
)
RETURNS 
@Return TABLE 
(
	TagName nvarchar(max) not null,
	PeopleWhoCompletedTag int not null,
	LastEntryTs datetimeoffset not null
)
AS
BEGIN

	with GroupedByTagAndPerson as
	(
		select
			n.TagName,
			n.RegisteredUserId,
			count(n.Id) [PersonCount],
			max(n.EntryTs) [LastEntryTs]
		from NoItemsInFilterEntry n
		group by n.TagName, n.RegisteredUserId
	),
	GroupedByPerson as
	(
		select
			g.TagName,
			count(1) [PeopleWhoCompletedTag],
			max(g.LastEntryTs) [LastEntryTs]
		from GroupedByTagAndPerson g
		group by g.TagName
		having count(1) >= @PersonThreshold
	)
	insert into @Return (TagName, PeopleWhoCompletedTag, LastEntryTs)
		select top (@MaxReturnEntries)
			g.TagName,
			g.PeopleWhoCompletedTag,
			g.LastEntryTs
		from GroupedByPerson g
		order by g.LastEntryTs desc

	
	RETURN 
END
GO
