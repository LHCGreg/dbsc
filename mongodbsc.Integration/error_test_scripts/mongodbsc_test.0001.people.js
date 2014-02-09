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