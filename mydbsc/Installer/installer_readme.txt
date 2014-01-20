Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed.

There is a "BuildInstaller" build target for this project.

msbuild mydbsc.csproj /target:BuildInstaller

Files to be included in the installer must be manually added to installer.wxs. **THIS MEANS IF YOU ADD OR REMOVE A REFERENCE, YOU MUST UPDATE installer.wxs!**