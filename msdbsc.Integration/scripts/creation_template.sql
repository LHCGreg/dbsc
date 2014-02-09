CREATE DATABASE $DatabaseName$;
GO

USE $DatabaseName$;

CREATE TABLE creation_template_ran
(
	id int NOT NULL PRIMARY KEY
);
GO

INSERT INTO creation_template_ran
(id)
VALUES
(5);