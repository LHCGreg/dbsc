-- Oracle SQL scripts cannot contain a UTF-8 BOM
CREATE TABLE person
(
	person_id number(7,0) NOT NULL PRIMARY KEY,
	name nvarchar2(128) NOT NULL,
	birthday date NOT NULL,
	default_test number(7,0) DEFAULT 42 NULL
);

CREATE UNIQUE INDEX ix_person__name ON person (name);

INSERT INTO person
(person_id, name, birthday, default_test)
VALUES
(1, 'George R.R. Martin', TO_DATE ('1948-09-20', 'YYYY-MM-DD'), 5);

INSERT INTO person
(person_id, name, birthday)
VALUES
(2, 'Greg', TO_DATE ('2012-06-07', 'YYYY-MM-DD'));

-- No need to test isolation, scripts are run in separate instances of sqlplus.
CREATE TABLE script_isolation_test
(
	step number(7, 0) NOT NULL PRIMARY KEY,
	val nvarchar2(128) NOT NULL
);

INSERT INTO script_isolation_test
(step, val)
VALUES
(1, 'x');