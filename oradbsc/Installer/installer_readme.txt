Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed.

There is an oradbsc.Installer folder with oradbsc.Installer.wixproj and oradbsc.Bundle.wixproj projects. The installer builds the oradbsc MSI, the bundle builds a .exe installer that installs .NET 4 first if it is not already installed, then installs oradbsc.

msbuild oradbsc.Bundle.wixproj

Files to be included in the installer must be manually added to installer.wxs. **THIS MEANS IF YOU ADD OR REMOVE A REFERENCE, YOU MUST UPDATE installer.wxs!**