Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed.

There is a mydbsc.Installer folder with mydbsc.Installer.wixproj and mydbsc.Bundle.wixproj projects. The installer builds the mydbsc MSI, the bundle builds a .exe installer that installs .NET 4 first if it is not already installed, then installs mydbsc.

msbuild mydbsc.bundle.wixproj

Files to be included in the installer must be manually added to installer.wxs. **THIS MEANS IF YOU ADD OR REMOVE A REFERENCE, YOU MUST UPDATE installer.wxs!**