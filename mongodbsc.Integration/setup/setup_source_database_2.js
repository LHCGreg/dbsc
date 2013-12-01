db = db.getSiblingDB('admin');
db.addUser( { user: "useradmin", pwd: "testpw", roles: ["userAdminAnyDatabase", "readWriteAnyDatabase", "dbAdminAnyDatabase", "clusterAdmin"] } );