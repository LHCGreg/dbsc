To run these integration tests, you must have SQL Server running locally such that connecting to just "localhost" and not "localhost\MSSQLSERVER" or "localhost\SQLEXPRESS"  connects to it.

You must have a user with the username "dbsc_test_user" and a password of "testpw" with permission to create databases. The Windows user running the tests must be a login on the server.

There must be a database called msdbsc_test_source that dbsc_test_user and the Windows user running the tests have access to that has been initialized with setup/setup_source_database.sql.

There must be a database called msdbsc_test_source_2 that dbsc_test_user and the Windows user running the tests have access to that has been initialized with setup/setup_source_database_2.sql.