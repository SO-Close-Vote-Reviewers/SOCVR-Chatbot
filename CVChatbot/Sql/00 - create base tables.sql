
create table RegistedUserPrivilageLevel
(
	Id int not null primary key identity,
	Name nvarchar(50) not null unique
)

insert into RegistedUserPrivilageLevel (Name) values ('Operator');
insert into RegistedUserPrivilageLevel (Name) values ('Owner');

create table RegisteredUser
(
	Id int not null primary key identity,
	ChatProfileId int not null unique,
	--UserName nvarchar(max) not null,
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
create table NoItemsInFilterNotification
(
	Id int not null primary key identity,
	RegisteredUserId int not null foreign key references RegisteredUser(Id),
	TagName nvarchar(max) not null,
	EntryDate datetimeoffset not null
)