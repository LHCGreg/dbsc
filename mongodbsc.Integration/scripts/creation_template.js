if (db.getName() === "admin") {
	db = db.getSiblingDB("creation_template_test");
	db.testCollection.save( { _id: 1, pass: true } );
}
else {
	db = db.getSiblingDB("creation_template_test");
	db.testCollection.save( { _id: 1, pass: false } );
}