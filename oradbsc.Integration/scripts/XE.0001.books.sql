CREATE TABLE book
(
	book_id number(7, 0) NOT NULL PRIMARY KEY,
	title nvarchar2(256) NOT NULL,
	subtitle nvarchar2(256) NULL,
	author_person_id number(7, 0) NOT NULL REFERENCES person (person_id)
);

INSERT INTO book
(book_id, title, subtitle, author_person_id)
VALUES
(1, 'A Game of Thrones', NULL, 1);