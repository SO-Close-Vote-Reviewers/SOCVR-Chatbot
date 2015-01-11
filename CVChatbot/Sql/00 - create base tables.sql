
create table RegisteredUser
(
	Id int not null primary key identity,
	ChatProfileId int not null unique,
	IsOwner bit not null default((0))
)

-- when a person says "they are starting" till they either run out of close votes or review items
create table ReviewSession
(
	Id int not null primary key identity,
	RegisteredUserId int not null foreign key references RegisteredUser(Id),
	SessionStart datetimeoffset not null,
	SessionEnd datetimeoffset null,
	ItemsReviewed int null
)

--track when a user finishes a tag, multiple entries of the same tag by same user is allowed.
--most of the time you will just look for the most recent entry
create table NoItemsInFilterEntry
(
	Id int not null primary key identity,
	RegisteredUserId int not null foreign key references RegisteredUser(Id),
	TagName nvarchar(max) not null,
	EntryTs datetimeoffset not null
)

create table CompletedAuditEntry
(
	Id int not null primary key identity,
	RegisteredUserId int not null foreign key references RegisteredUser(Id),
	TagName nvarchar(max) not null,
	EntryTs datetimeoffset not null
)