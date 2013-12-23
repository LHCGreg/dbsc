BEGIN TRANSACTION;

CREATE TABLE IF NOT EXISTS person
(
	person_id serial NOT NULL PRIMARY KEY,
	name text NOT NULL,
	birthday date NOT NULL,
	default_test int NULL DEFAULT 42
);

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

INSERT INTO script_isolation_test
(step, val)
SELECT 0 AS step, setting AS val
FROM pg_settings
WHERE name = 'enable_seqscan';
-- should be "on"

SET ENABLE_SEQSCAN = off;

INSERT INTO script_isolation_test
(step, val)
SELECT 1 AS step, setting AS val
FROM pg_settings
WHERE name = 'enable_seqscan';
-- should be "off"

COMMIT TRANSACTION;