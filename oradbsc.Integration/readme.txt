To run these integration tests, you must have Oracle XE running locally on the standard port.

You must have a user with the username "dbsc_test_user" and a password of "testpw" with permission to create databases. You can accomplish this with:

create user dbsc_test_user identified by testpw;
grant connect, resource to dbsc_test_user;