BEGIN TRANSACTION;

INSERT INTO person
(name, birthday, default_test)
VALUES
('Mike', '2013-05-11', NULL);

COMMIT TRANSACTION;