Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed and the WiX bin folder in your PATH.

You must have the x86 SQL Management Objects 11 installer (ENU\x86\SharedManagementObjects.msi at http://www.microsoft.com/en-us/download/details.aspx?id=35580) and x86 SQL 2012 CLR Types ( somewhere on your machine (not included in the repo to avoid putting a 6 MB binary blob in the repo).

There is a "BuildInstaller" build target for this project.

msbuild msdbsc.csproj /target:BuildInstaller /p:Platform=x86 /p:Configuration=Release "/p:SMO11x86MSI=<path to x86 SMO 11 msi installer>" "/p:SqlClrTypes2012x86MSI=<path to x86 SQL CLR Types 2012 msi installer>"