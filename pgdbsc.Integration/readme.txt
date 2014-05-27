To run these integration tests, you must have PostgreSQL running locally on the standard port.

You must have a user with the username "dbsc_test_user" and a password of "testpw" with permission to create databases. You can accomplish this with:

CREATE USER dbsc_test_user CREATEDB PASSWORD 'testpw';

There must be a database called pgdbsc_test_source that dbsc_test_user has access to that has been initialized with setup/setup_source_database.sql.

There must be a database called pgdbsc_test_source_2 that dbsc_test_user has access to that has been initialized with setup/setup_source_database_2.sql.

If running the tests on Windows, you must have a user with the username "dbsc_test_user_sspi" that has permission to create databases, can be authenticated to with SSPI with your Windows username, and that has read access to pgdbsc_test_source and pgdbsc_test_source_2. You can create the user by logging in as postgres (psql -h localhost -U postgres)

CREATE USER dbsc_test_user_sspi WITH CREATEDB;
\q

Then grant it access to pgdbsc_test_source and pgdbsc_test_source_2:

psql -h localhost -U postgres -d pgdbsc_test_source
GRANT SELECT ON ALL TABLES IN SCHEMA public TO dbsc_test_user_sspi;
\q
psql -h localhost -U postgres -d pgdbsc_test_source_2
GRANT SELECT ON ALL TABLES IN SCHEMA public TO dbsc_test_user_sspi;
\q


To add SSPI authentication, add this line to pg_ident.conf:

dbsc			Your_Windows_Username					dbsc_test_user_sspi

and add these lines to pg_hba.conf ABOVE ALL OTHER CONFIGURATION LINES

host	all				dbsc_test_user_sspi	127.0.0.1/32		sspi	map=dbsc
host	all				dbsc_test_user_sspi	::1/128				sspi	map=dbsc