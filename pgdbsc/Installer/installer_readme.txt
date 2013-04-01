Building the installer requires you to have the WiX Toolset (http://wixtoolset.org/) installed and the WiX bin folder in your PATH.

There is a "BuildInstaller" build target for this project.

msbuild pgdbsc.csproj /target:BuildInstaller /p:Platform=AnyCPU /p:Configuration=Release