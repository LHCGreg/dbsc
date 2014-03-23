BUILD DEPENDENCIES
------------------
DBSC uses NuGet to fetch most dependencies when building.

If you are on Windows, you must have version 3.7 or higher (but less than 4.0?) of the WiX Toolset (http://wixtoolset.org/) installed, even if you are not building an installer.


DEPLOYMENT CONSIDERATIONS
-------------------------
Take care when adding references. Non-system references that must be present at runtime must be explicitly included in the Windows installer (Installer/installer.wxs). Linux packages will automatically pick up *.dll, *.exe, and *.config from the bin folder. License files must be explicitly included in both Windows installers and Linux packages. They are added to the Windows installer in Installer/installer.wxs and to Linux packages in pgdbsc.targets (or whatever.targets).

The Mono Linux packages are very granular with system libraries so even something innocent like System.Xml.dll must be listed as a package dependency. When adding new non-system dependencies, check what its system dependencies are using a decompiler like JetBrains DotPeek (http://www.jetbrains.com/decompiler/). Recursively check the system dependencies of its non-system dependencies as well.

The version number to be used for executable projects is stored in version.xml so that it can be read during the build process and used when building the installer, Debian package, and zip package.

msdbsc can only be built x86, not AnyCPU or x64 due to dependencies on 32-bit SQL Server native dlls.


BUILDING A ZIP PACKAGE
----------------------
You can build a .zip package using the BuildZipPackage MSBuild target.

msbuild /t:BuildZipPackage /p:Configuration=Release;Platform=AnyCPU pgdbsc.csproj

The .zip will be placed in ZipReleases/ in the project folder. The .zip will contain the contents of the bin folder except *.pdb, *.mdb, and *.xml, and any *.vshost.exe that might be there.


BUILDING AN MSI WINDOWS INSTALLER
---------------------------------
See Installer/installer_readme.txt for the project. You must have the WiX binaries on your path.


RUNNING THE AUTOMATED TESTS
---------------------------
You must have NUnit 2.6.2 or higher installed and the NUnit binaries on your path. Projects called *.Tests are unit tests with no environmental dependencies. Projects called *.Integration are integration tests that require a database server to be specially set up.

Integration test projects have a readme.txt explaining setup requirements.

Test projects have a run_tests.bat and run_tests.sh script that will build the project and run the tests.



BUILDING ON LINUX WITH MONO
---------------------------
You must have the mono-complete package (Debian) or similar installed. You must have Mono version 3.0.6 or higher to build, but Mono 2.10.8.1 is the minimum supported end-user version. The higher version for build is required for NuGet to work properly. Currently Debian and Ubuntu users must get Mono 3.0.6 from a ppa or Debian Testing (Jessie).

HOW TO INSTALL MONO FROM DEBIAN TESTING ON DEBIAN STABLE
--------------------------------------------------------
Add this line to /etc/apt/apt.conf (create it if it does not already exist)

APT::Default-Release "stable";

Create /etc/apt/sources.list.d/testing.list with these contents:

deb http://http.us.debian.org/debian jessie main
deb-src http://http.us.debian.org/debian jessie main

deb http://security.debian.org/ jessie/updates main
deb-src http://security.debian.org/ jessie/updates main

This lets you install selected packages from Testing without defaulting everything to Testing.

You can then use

sudo apt-get install -t testing mono-complete

INSTALLAING ROOT CERTIFICATES INTO MONO'S CERTIFICATE STORE
-----------------------------------------------------------
For NuGet to be able to download packages you must install root certificates into Mono's certificate store by running

mozroots --import --sync

MONO DEPENDENCY NOTES
---------------------
When building on non-Windows systems, pgdbsc will rely on Mono.Security being in the GAC. It is a dependency of Npgsql but is not needed by pgdbsc itself, so it is not actually referenced when building on non-Windows systems.

Why not use the system Npgsql? Because the Debian Npgsql package appears to be using an absolutely ancient version and uses the Mono version for the assembly version instead of the actual Npgsql version number.

BUILDING THE DEBIAN PACKAGE
---------------------------
To build the Debian package, you must be running Debian and have fpm (https://github.com/jordansissel/fpm) and the lintian package installed.

To build the Debian package, run

xbuild /t:BuildDebianPackage "/p:Configuration=Release;Platform=AnyCPU" pgdbsc.csproj

(or whatever.csproj).

The built package will be in deb/bin/package_name_<version>.deb. Lintian is run as part of the build process. linux_common/lintian_suppress.txt contains lintian warnings/errors to ignore.