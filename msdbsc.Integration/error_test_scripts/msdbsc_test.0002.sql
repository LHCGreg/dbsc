set xact_abort on;

BEGIN TRANSACTION

INSERT INTO person
(name, birthday, default_test)
VALUES
('Mike', '2013-05-11', NULL);

-- Insert violates unique index
INSERT INTO person
(name, birthday, default_test)
VALUES
('Mike', '2013-05-11', NULL);

INSERT INTO person
(name, birthday, default_test)
VALUES
('Bob', '2013-05-11', NULL);

COMMIT TRANSACTION