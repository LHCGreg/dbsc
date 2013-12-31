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