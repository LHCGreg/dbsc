BEGIN TRANSACTION;

CREATE TABLE person
(
	person_id serial NOT NULL PRIMARY KEY,
	name text NOT NULL,
	birthday date NOT NULL,
	default_test int NULL DEFAULT 42
);

CREATE UNIQUE INDEX ix_person__name ON person (name);

INSERT INTO person
(name, birthday, default_test)
VALUES
('George R.R. Martin', '1948-09-20', 5),
('Christina', '2012-02-03', NULL);

CREATE TABLE script_isolation_test
(
	step int NOT NULL PRIMARY KEY,
	val text NOT NULL
);

INSERT INTO script_isolation_test
(step, val)
VALUES
(0, 'on'),
(1, 'off'),
(2, 'on');

CREATE TABLE book
(
	book_id serial NOT NULL PRIMARY KEY,
	title text NOT NULL,
	subtitle text NULL,
	author_person_id int REFERENCES person (person_id)
);

INSERT INTO book
(title, subtitle, author_person_id)
VALUES
('A Game of Thrones', NULL, (SELECT person_id FROM person WHERE name = 'George R.R. Martin'));

CREATE TABLE dbsc_metadata
(
    property_name text NOT NULL PRIMARY KEY,
    property_value text
);

INSERT INTO dbsc_metadata
(property_name, property_value)
VALUES
('MasterDatabaseName', 'pgdbsc_test'),
('Version', '1'),
('LastChangeUTC', '2013-12-22T04:01:48');

COMMIT TRANSACTION;