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
		}
	]
);