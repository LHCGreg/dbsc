#!/bin/sh
xbuild /t:Build "/p:Configuration=Debug;Platform=x86" mydbsc.Integration.csproj
nunit-console "bin/Debug_x86/mydbsc.Integration.dll" -nologo -noxml "-out=bin/Debug_x86/test_stdout.txt" "-err=bin/Debug_x86/test_stderr.txt"