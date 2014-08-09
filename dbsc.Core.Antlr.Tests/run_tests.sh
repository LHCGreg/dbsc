#!/bin/sh
set -e
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" dbsc.Core.Antlr.Tests.csproj
nunit-console "bin/Debug_AnyCPU/dbsc.Core.Antlr.Tests.dll" -nologo -noxml "-out=bin/Debug_AnyCPU/test_stdout.txt" "-err=bin/Debug/test_stderr.txt"
