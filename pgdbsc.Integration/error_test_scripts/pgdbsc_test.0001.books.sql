BEGIN TRANSACTION;

INSERT INTO script_isolation_test
(step, val)
SELECT 2 AS step, setting AS val
FROM pg_settings
WHERE name = 'enable_seqscan';
-- should be "on"

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

COMMIT TRANSACTION;