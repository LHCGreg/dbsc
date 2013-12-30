// Run this on your local mongo instance by starting the Mongo shell and running
// load("path/to/this/file/setup_source_database_2.js");

db = db.getSiblingDB("mongodbsc_test_source_2");

db.createCollection('books');
db.books.ensureIndex({ name: 1 });
db.books.insert(
	[
		{
			name: 'Charlie and the Chocolate Factory',
			author: 'Roald Dahl'
		},
		{
			name: 'A Feast for Crows',
			author: 'George R.R. Martin'
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

db.createCollection('dbsc_metadata');
db.dbsc_metadata.insert(
	{
		"_id" : 1,
		"Version" : 1,
		"MasterDatabaseName" : "mongodbsc_test",
		"LastChangeUTC" : ISODate("2013-11-18T01:48:55.17Z")
	}
);