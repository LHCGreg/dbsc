#!/bin/sh
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" mongodbsc.Integration/mongodbsc.Integration.csproj
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" pgdbsc.Integration/pgdbsc.Integration.csproj
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" mydbsc.Integration/mydbsc.Integration.csproj
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" oradbsc.Integration/oradbsc.Integration.csproj
xbuild /t:Build "/p:Configuration=Debug;Platform=AnyCPU" dbsc.Core.Tests/dbsc.Core.Tests.csproj

nunit-console "dbsc.Core.Tests/bin/Debug_AnyCPU/dbsc.Core.Tests.dll" "mongodbsc.Integration/bin/Debug_AnyCPU/mongodbsc.Integration.dll" "pgdbsc.Integration/bin/Debug_AnyCPU/pgdbsc.Integration.dll" "mydbsc.Integration/bin/Debug_AnyCPU/mydbsc.Integration.dll" "oradbsc.Integration/bin/Debug_AnyCPU/oradbsc.Integration.dll" -nologo -noxml "-out=test_stdout.txt" "-err=test_stderr.txt"