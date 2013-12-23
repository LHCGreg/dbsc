To run these integration tests, you must have MongoDB running locally on the standard port.

Run setup_source_database.js on your local mongod by starting the mongo shell and running

load("path/to/setup_source_database.js");

Run setup_source_database_2.js on your local mongod by stating the mongo shell and running

load("path/to/setup_source_database_2.js");

You must have a second mongod running on port 30017 with authentication enabled (auth = true in the config).

On the second mongod, start a mongo shell conecting to this other mongod (mongo --port 30017) and run

load("path/to/setup_auth_source_database.js");

mongo and mongodump must be on your PATH.