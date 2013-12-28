To run these integration tests, you must have MySQL running locally on the standard port.

You must have a user with the username "dbsc_test_user" and a password of "testpw" with permission to create databases. You can accomplish this with:

CREATE USER 'dbsc_test_user'@'localhost' IDENTIFIED BY 'testpw';
GRANT ALL ON *.* TO 'dbsc_test_user'@'localhost';

There must be a database called mydbsc_test_source that dbsc_test_user has access to that has been initialized with setup/setup_source_database.sql.

There must be a database called mydbsc_test_source_2 that dbsc_test_user has access to that has been initialized with setup/setup_source_database_2.sql.