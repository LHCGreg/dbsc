// Run this on your second local mongo instance on port 30017 by starting the Mongo shell with
// mongo --port 30017
// and running
// load("path/to/this/file/setup_auth_source_database.js");

db = db.getSiblingDB('admin');
db.addUser( { user: "useradmin", pwd: "testpw", roles: ["userAdminAnyDatabase", "readWriteAnyDatabase", "dbAdminAnyDatabase", "clusterAdmin"] } );
db = db.getSiblingDB('mongodbsc_test_source');

db.createCollection('books');
db.books.ensureIndex({ name: 1 });
db.books.insert(
	[
		{
			name: 'Charlie and the Chocolate Factory',
			author: 'Roald Dahl'
		}
	]
);

db.createCollection('people');
db.people.insert(
	[
		{
			name: 'Greg',
			preferences: {
				a: 500,
				b: [800, 900],
				c: true
			}
		},
		{
			name: 'Joe',
			preferences: {
				a: 1000,
				b: null,
				c: false
			}
		}
	]
);

db.createCollection('numbers');
db.numbers.insert(
	[
		{
			num: 1,
			english: 'one',
			spanish: 'uno'
		},
		{
			num: 2,
			english: 'two',
			spanish: 'dos'
		}
	]
);

db.createCollection('dbsc_metadata');
db.dbsc_metadata.insert(
	{
		"_id" : 1,
		"Version" : 2,
		"MasterDatabaseName" : "mongodbsc_test",
		"LastChangeUTC" : ISODate("2013-11-18T01:48:55.17Z")
	}
);