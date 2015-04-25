SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create FUNCTION GetUserAuditStats
(
	@ChatProfileId int
)
RETURNS 
@Return TABLE 
(
	TagName nvarchar(max) not null,
	[Percent] decimal(8, 1) not null,
	[Count] int not null
)
AS
BEGIN
	-- Fill the table variable with the rows for your result set
	
	declare @totalAuditCount int = (
		select count(1)
		from CompletedAuditEntry a
		inner join RegisteredUser r on a.RegisteredUserId = r.Id
		where r.ChatProfileId = @ChatProfileId);

	;with GroupedData as
	(
		select
			a.TagName,
			count(1) [TagCount]
		from CompletedAuditEntry a
		inner join RegisteredUser r on a.RegisteredUserId = r.Id
		where r.ChatProfileId = @ChatProfileId
		group by a.TagName
	),
	PercentData as
	(
		select
			gd.TagName,
			gd.TagCount,
			(gd.TagCount * 1.0 / @totalAuditCount * 100) [Percent]
		from GroupedData gd
	)
	insert into @Return (TagName, [Percent], [Count])
		select
			pd.TagName,
			pd.[Percent],
			pd.TagCount
		from PercentData pd

	RETURN 
END
GO