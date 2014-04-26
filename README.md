Complete documentation is available at https://github.com/LHCgreg/dbsc/wiki

dbsc (DataBase Source Control) is a command line tool for helping you create, recreate, and update your database. It works by running a series of numbered SQL scripts that you write, giving you access to the full power of SQL. It keeps track of what revision the database is on, allowing you to easily get your changes to the rest of your team and deploy updates to QA and production.

When creating or updating a database, dbsc allows you to import data from another database. When the database being created or updated reaches the revision that the other database is on, dbsc will clear the tables in the database being created or updated and import data for all tables from the other database. This makes it a snap to bring in test data.

# Quick Introduction

Say a new developer joins your team. Your project uses a PostgreSQL database. Developers use a local database for testing and debugging. He checks out your project's code from source control. He opens a command prompt in a directory with 100 scripts named project_x.0000.sql, project_x.0001.sql, project_x.0002.sql, etc.

```
[mike@mikes_computer db_scripts]$ pgdbsc checkout -u mike
Creating database project_x on localhost.
Created database project_x on localhost.
Updating to r0
Updating to r1
Updating to r1
...
Updating to r100
At revision 100
[mike@mikes_computer db_scripts]$
```

To copy data from the QA database, Mike can run

```
[mike@mikes_computer db_scripts]$ pgdbsc checkout -u mike -sourceDbServer qa-pg.mycompany.local -sourceUsername mike
```

When his coworkers make database changes, Mike gets the latest code from source control and runs

```
[mike@mikes_computer db_scripts]$ pgdbsc update -u mike
Updating to r101
Updating to r102
At revision 102
[mike@mikes_computer db_scripts]$
```

Check out the [tutorial](https://github.com/LHCGreg/dbsc/wiki/Tutorial) for a more detailed introduction.

# Installing

dbsc comes in different flavors for different database engines. dbsc currently supports

* PostgreSQL (pgdbsc)
* Microsoft SQL Server (msdbsc)
* MySQL (mydbsc)
* MongoDB (mongodbsc)

dbsc runs on Windows and (except for msdbsc) Linux and Mac OS X using Mono.

There are .exe installers and .zip archives on the [releases](https://github.com/LHCGreg/dbsc/releases) page. The .exe installers will install the given flavor of dbsc along with anything it needs to run and will add the installed version of dbsc to your PATH environment variable. This is the recommended way of installing in Windows.

Packages for Debian and Debian-based Linux distributions (such as Ubuntu) are available.

mongodbsc depends on the Mongo shell to run scripts. You may wish to follow the instructions at http://docs.mongodb.org/manual/tutorial/install-mongodb-on-debian/ or http://docs.mongodb.org/manual/tutorial/install-mongodb-on-ubuntu/ to add the mongodb.org APT repository to your system as a package source. This way you will get the latest stable release from mongodb.org instead of the (likely old) version that your distro repository has.

```
wget -O - http://apt.dbsourcecontrol.org/keys/gregnajda@gmail.com.gpg.key | sudo apt-key add -
echo 'deb http://apt.dbsourcecontrol.org dbsc main' | sudo tee /etc/apt/sources.list.d/dbsc.list
sudo apt-get update
sudo apt-get install pgdbsc # for the PostgreSQL version
sudo apt-get install mydbsc # for the MySQL version
sudo apt-get install mongodbsc # for the MongoDB version
```

Packages for Red Hat, CentOS, and Fedora are also available. You must have a version of mono-core of at least 2.10.8 available. At the time of this writing (2014-04-25), only Fedora has a version that meets that requirement in the default repositories. On CentOS and Red Hat you will have to find a version of Mono released within the last 3 years somewhere.

mongodbsc depends on the Mongo shell to run scripts. To get the official mongodb.org package instead of packages from your RH-family distro, you may wish to follow the instructions at http://docs.mongodb.org/manual/tutorial/install-mongodb-on-red-hat-centos-or-fedora-linux/ and install the mongodb-org-shell package.

```
sudo yum-config-manager --add-repo http://rpm.dbsourcecontrol.org/dbsc.repo
sudo yum makecache
sudo yum install pgdbsc # for the PostgreSQL version
sudo yum install mydbsc # for the MySQL version
sudo yum install mongodbsc # for the MongoDB version
```

On other Linux distributions and Unix systems and Mac OS X you can download a .zip from the [releases](https://github.com/LHCGreg/dbsc/releases) page. You will need to have mono 2.10.8 or higher installed and run "mono <path to dbsc> <command line arguments>" or make a shell script to do that for you.

# Getting help

You can get the available command-line options with

`pgdbsc -h`

or

`msdbsc -h`

or

`mydbsc -h`

or

`mongodbsc -h`

depending on which flavor you are using.

# Changelog

### msdbsc 2.1.0
* When using -importTableList to import data for only certain tables, don't clear tables that are not being imported.

### pgdbsc 2.1.0
* When using -importTableList to import data for only certain tables, don't clear tables that are not being imported.

### mydbsc 2.1.0
* When using -importTableList to import data for only certain tables, don't clear tables that are not being imported.

### mongodbsc 2.1.0
* When using -importTableList to import data for only certain tables, don't clear tables that are not being imported.