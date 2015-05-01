create table public."UnrecognizedCommand"
(
	"Id" serial,
	"Command" varchar(1000) not null
)
WITH (
  OIDS = FALSE
)
;