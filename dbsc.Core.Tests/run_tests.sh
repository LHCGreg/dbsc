#!/bin/sh
set -e
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" dbsc.Core.Tests.csproj
nunit-console "bin/Debug_AnyCPU/dbsc.Core.Tests.dll" -nologo -noxml "-out=bin/Debug_AnyCPU/test_stdout.txt" "-err=bin/Debug_AnyCPU/test_stderr.txt"