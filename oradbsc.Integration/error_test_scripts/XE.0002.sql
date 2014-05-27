-- Reuse ID, should get an error

INSERT INTO person
(person_id, name, birthday, default_test)
VALUES
(2, 'Mike', TO_DATE('2013-05-11', 'YYYY-MM-DD'), NULL);