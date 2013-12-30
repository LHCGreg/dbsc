INSERT INTO script_isolation_test
(step, val)
VALUES
(2, CONVERT(@@session.auto_increment_increment, char(1)));
-- should be "1"

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