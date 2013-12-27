BEGIN TRANSACTION

-- should be on
IF (SELECT @@OPTIONS & 32) = 32
BEGIN
	INSERT INTO script_isolation_test
	(step, val)
	VALUES
	(2, 'on');
END
ELSE
BEGIN
	INSERT INTO script_isolation_test
	(step, val)
	VALUES
	(2, 'off');
END

CREATE TABLE book
(
	book_id int NOT NULL IDENTITY PRIMARY KEY,
	title nvarchar(128) NOT NULL,
	subtitle nvarchar(128) NULL,
	author_person_id int NOT NULL REFERENCES person (person_id)
);

GO

INSERT INTO book
(title, subtitle, author_person_id)
VALUES
('A Game of Thrones', NULL, (SELECT person_id FROM person WHERE name = 'George R.R. Martin'));

COMMIT TRANSACTION