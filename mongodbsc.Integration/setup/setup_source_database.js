// Run this on mongodbsc_test_source

db.createCollection('books');
db.books.ensureIndex({ name: 1 });
db.books.insert(
	[
		{
			name: 'A Game of Thrones',
			author: 'George R.R. Martin'
		},
		{
			name: 'Clean Code',
			author: 'Robert C. Martin'
		},
		{
			name: 'The Mythical Man-Month',
			author: 'Frederick P. Brooks, Jr.'
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