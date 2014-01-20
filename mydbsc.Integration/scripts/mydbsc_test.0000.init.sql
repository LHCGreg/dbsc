CREATE TABLE person
(
	person_id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	name varchar(128) NOT NULL,
	birthday date NOT NULL,
	default_test int NULL DEFAULT 42
)
ENGINE=InnoDB;

CREATE UNIQUE INDEX ix_person__name ON person (name);

-- George R.R. Martin must have the same ID it has in the source database to simplify the test for importing only certain tables.

INSERT INTO person
(name, birthday, default_test)
VALUES
('George R.R. Martin', '1948-09-20', 5);

INSERT INTO person
(name, birthday)
VALUES
('Greg', '2012-06-07');

CREATE TABLE script_isolation_test
(
	step int NOT NULL PRIMARY KEY,
	val char(1) NOT NULL
)
ENGINE=InnoDB;

INSERT INTO script_isolation_test
(step, val)
VALUES
(0, CONVERT(@@session.auto_increment_increment, char(1)));
-- should be "1"

SET @@session.auto_increment_increment=5;

INSERT INTO script_isolation_test
(step, val)
VALUES
(1, CONVERT(@@session.auto_increment_increment, char(1)));
-- should be "5"