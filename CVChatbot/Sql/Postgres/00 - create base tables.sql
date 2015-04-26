CREATE TABLE public."RegisteredUser"
(
   "Id" SERIAL,
   "ChatProfileId" integer NOT NULL, 
   "IsOwner" boolean NOT NULL DEFAULT FALSE, 
   CONSTRAINT "pk_RegisteredUser" PRIMARY KEY ("Id"), 
   CONSTRAINT "UN_RegisteredUser_ChatProfileId" UNIQUE ("ChatProfileId")
) 
WITH (
  OIDS = FALSE
)
;

CREATE TABLE public."ReviewSession"
(
	"Id" SERIAL,
	"RegisteredUserId" integer NOT NULL,
	"SessionStart" timestamptz not null,
	"SessionEnd" timestamptz null,
	"ItemsReviewed" integer null,
	
	CONSTRAINT "pk_ReviewSession" PRIMARY KEY ("Id"), 
	CONSTRAINT "fk_ReviewSession_RegisteredUser" foreign key ("Id") references "RegisteredUser" ("Id")
)
WITH (
  OIDS = FALSE
)
;

create table public."NoItemsInFilterEntry"
(
	"Id" SERIAL,
	"RegisteredUserId" integer NOT NULL,
	"TagName" varchar(256) not null,
	"EntryTs" timestamptz not null,
	
	CONSTRAINT "pk_NoItemsInFilterEntry" PRIMARY KEY ("Id"), 
	CONSTRAINT "fk_NoItemsInFilterEntry_RegisteredUser" foreign key ("RegisteredUserId") references "RegisteredUser" ("Id")
)
WITH (
  OIDS = FALSE
)
;

create table public."CompletedAuditEntry"
(
	"Id" SERIAL,
	"RegisteredUserId" integer NOT NULL,
	"TagName" varchar(256) not null,
	"EntryTs" timestamptz not null,
	
	CONSTRAINT "pk_CompletedAuditEntry" PRIMARY KEY ("Id"), 
	CONSTRAINT "fk_CompletedAuditEntry_RegisteredUser" foreign key ("RegisteredUserId") references "RegisteredUser" ("Id")
)
WITH (
  OIDS = FALSE
)
;

