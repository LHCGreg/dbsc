CREATE DATABASE $DatabaseName$;

USE $DatabaseName$;

CREATE TABLE creation_template_ran
(
	id int NOT NULL PRIMARY KEY
);

INSERT INTO creation_template_ran
(id)
VALUES
(5);