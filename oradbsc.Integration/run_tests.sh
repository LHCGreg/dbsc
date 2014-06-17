#!/bin/sh
xbuild /t:Build "/p:Configuration=DebugMono;Platform=AnyCPU" oradbsc.Integration.csproj
nunit-console "bin/DebugMono_AnyCPU/oradbsc.Integration.dll" -nologo -noxml "-out=bin/DebugMono_AnyCPU/test_stdout.txt" "-err=bin/DebugMono_AnyCPU/test_stderr.txt"