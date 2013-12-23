To run these integration tests, you must have PostgreSQL running locally on the standard port.

You must have a user with the username "dbsc_test_user" and a password of "testpw" with permission to create databases. You can accomplish this with:

CREATE USER dbsc_test_user CREATEDB PASSWORD 'testpw'

There must be a database called pgdbsc_test_source that dbsc_test_user has access to that has been initialized with setup/setup_source_database.sql.