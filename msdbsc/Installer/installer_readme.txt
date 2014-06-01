Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed.

You must have the x86 SQL Management Objects 11 installer (ENU\x86\SharedManagementObjects.msi at http://www.microsoft.com/en-us/download/details.aspx?id=35580) and x86 SQL 2012 CLR Types somewhere on your machine (not included in the repo to avoid putting a 6 MB binary blob in the repo).

There is a msdbsc.Installer folder with msdbsc.Installer.wixproj and msdbsc.Bundle.wixproj projects. The installer builds the msdbsc MSI, the bundle builds a .exe installer that installs .NET 4, SQL Management Objects, and SQL CLR Types first if they are not alreadyinstalled, then installs msdbsc.

msbuild msdbsc.Bundle.wixproj "/p:SMO11x86MSI=<path to x86 SMO 11 msi installer>" "/p:SqlClrTypes2012x86MSI=<path to x86 SQL CLR Types 2012 msi installer>"

Files to be included in the installer must be manually added to installer.wxs. **THIS MEANS IF YOU ADD OR REMOVE A REFERENCE, YOU MUST UPDATE installer.wxs!**