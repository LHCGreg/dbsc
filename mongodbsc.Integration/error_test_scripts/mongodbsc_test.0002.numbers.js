throw "This is a test exception";

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