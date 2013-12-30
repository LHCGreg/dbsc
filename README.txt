Complete documentation is available at https://github.com/LHCgreg/dbsc/wiki

dbsc (DataBase Source Control) is a command line tool for helping you create, recreate, and update your database. It works by running a series of numbered SQL scripts that you write, giving you access to the full power of SQL. It keeps track of what revision the database is on, allowing you to easily get your changes to the rest of your team and deploy updates to QA and production.

When creating or updating a database, dbsc allows you to import data from another database. When the database being created or updated reaches the revision that the other database is on, dbsc will clear the tables in the database being created or updated and import data for all tables from the other database. This makes it a snap to bring in test data.

dbsc comes in different flavors for different database engines. dbsc currently supports

* PostgreSQL (pgdbsc)
* Microsoft SQL Server (msdbsc)
* MySQL (mydbsc)
* MongoDB (mongodbsc)

dbsc runs on Windows and (except for msdbsc) Linux and Mac OS X using Mono.

The .exe installers will install the given flavor of dbsc along with anything it needs to run and will add the installed version of dbsc to your PATH environment variable. This is the recommended way of installing in Windows. On Linux and Mac OS X you can download the .zip package. You will need to have mono installed and run "mono <path to dbsc> <command line arguments>" or make a shell script to do that for you. Better support for non-Windows systems is planned for the future.

You can get the available command-line options with

pgdbsc -h
or
msdbsc -h
or
mydbsc -h
or
mongodbsc -h

depending on which flavor you are using.
