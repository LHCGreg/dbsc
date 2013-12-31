CREATE TABLE person
(
	person_id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	name varchar(128) NOT NULL,
	birthday date NOT NULL,
	default_test int NULL DEFAULT 42
)
ENGINE=InnoDB;

CREATE UNIQUE INDEX ix_person__name ON person (name);

INSERT INTO person
(name, birthday, default_test)
VALUES
('George R.R. Martin', '1948-09-20', 5);

CREATE TABLE script_isolation_test
(
	step int NOT NULL PRIMARY KEY,
	val char(1) NOT NULL
)
ENGINE=InnoDB;

INSERT INTO script_isolation_test
(step, val)
VALUES
(0, '1'),
(1, '5'),
(2, '1');

CREATE TABLE book
(
	book_id int NOT NULL AUTO_INCREMENT PRIMARY KEY,
	title varchar(128) NOT NULL,
	subtitle varchar(128) NULL,
	author_person_id int NOT NULL REFERENCES person (person_id)
)
ENGINE=InnoDB;

INSERT INTO book
(title, subtitle, author_person_id)
VALUES
('A Game of Thrones', NULL, (SELECT person_id FROM person WHERE name = 'George R.R. Martin'));

INSERT INTO person
(name, birthday, default_test)
VALUES
('Mike', '2013-05-11', NULL);

CREATE TABLE dbsc_metadata
(
    property_name nvarchar(128) NOT NULL PRIMARY KEY,
    property_value text
)
ENGINE=InnoDB;

INSERT INTO dbsc_metadata
(property_name, property_value)
VALUES
('MasterDatabaseName', 'mydbsc_test'),
('Version', '2'),
('LastChangeUTC', '2013-12-22T04:01:48');