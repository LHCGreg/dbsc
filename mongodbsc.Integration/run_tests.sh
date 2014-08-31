#!/bin/sh
set -e
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" mongodbsc.Integration.csproj
nunit-console "bin/Debug_AnyCPU/mongodbsc.Integration.dll" -nologo -noxml "-out=bin/Debug_AnyCPU/test_stdout.txt" "-err=bin/Debug_AnyCPU/test_stderr.txt"