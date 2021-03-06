msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU mongodbsc.Integration/mongodbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=x86 msdbsc.Integration/msdbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU pgdbsc.Integration/pgdbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU mydbsc.Integration/mydbsc.Integration.csproj
msbuild /t:Build /p:Configuration=DebugDotNet;Platform=AnyCPU oradbsc.Integration/oradbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU dbsc.Core.Tests/dbsc.Core.Tests.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU dbsc.Core.Antlr.Tests/dbsc.Core.Antlr.Tests.csproj

nunit-console "dbsc.Core.Tests/bin/Debug_AnyCPU/dbsc.Core.Tests.dll" "dbsc.Core.Antlr.Tests/bin/Debug_AnyCPU/dbsc.Core.Antlr.Tests.dll" "mongodbsc.Integration/bin/Debug_AnyCPU/mongodbsc.Integration.dll" "pgdbsc.Integration/bin/Debug_AnyCPU/pgdbsc.Integration.dll" "mydbsc.Integration/bin/Debug_AnyCPU/mydbsc.Integration.dll" "oradbsc.Integration/bin/DebugDotNet_AnyCPU/oradbsc.Integration.dll" -nologo -noxml "-out=test_stdout.txt" "-err=test_stderr.txt"

nunit-console-x86 "msdbsc.Integration/bin/Debug_x86/msdbsc.Integration.dll" -nologo -noxml "-out=msdbsc_test_stdout.txt" "-err=msdbsc_test_stderr.txt"