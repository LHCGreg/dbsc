Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed.

There is a pgdbsc.Installer folder with pgdbsc.Installer.wixproj and pgdbsc.Bundle.wixproj projects. The installer builds the pgdbsc MSI, the bundle builds a .exe installer that installs .NET 4 first if it is not already installed, then installs pgdbsc.

msbuild pgdbsc.Bundle.wixproj

Files to be included in the installer must be manually added to installer.wxs. **THIS MEANS IF YOU ADD OR REMOVE A REFERENCE, YOU MUST UPDATE installer.wxs!**