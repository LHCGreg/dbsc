BEGIN TRANSACTION

CREATE TABLE person
(
	person_id INT NOT NULL IDENTITY PRIMARY KEY,
	name nvarchar(128) NOT NULL,
	birthday date NOT NULL,
	default_test int NULL DEFAULT 42
);

GO

CREATE UNIQUE INDEX ix_person__name ON person (name);

INSERT INTO person
(name, birthday)
VALUES
('Greg', '2012-06-07');

INSERT INTO person
(name, birthday, default_test)
VALUES
('George R.R. Martin', '1948-09-20', 5);

CREATE TABLE script_isolation_test
(
	step int NOT NULL PRIMARY KEY,
	val text NOT NULL
);

GO

-- should be on
IF (SELECT @@OPTIONS & 32) = 32
BEGIN
	INSERT INTO script_isolation_test
	(step, val)
	VALUES
	(0, 'on');
END
ELSE
BEGIN
	INSERT INTO script_isolation_test
	(step, val)
	VALUES
	(0, 'off');
END

SET ANSI_NULLS OFF;

-- should be off
IF (SELECT @@OPTIONS & 32) = 32
BEGIN
	INSERT INTO script_isolation_test
	(step, val)
	VALUES
	(1, 'on');
END
ELSE
BEGIN
	INSERT INTO script_isolation_test
	(step, val)
	VALUES
	(1, 'off');
END

COMMIT TRANSACTION