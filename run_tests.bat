msbuild /t:Build /p:Configuration=Debug;Platform=x86 mongodbsc.Integration/mongodbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=x86 msdbsc.Integration/msdbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=x86 pgdbsc.Integration/pgdbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=x86 mydbsc.Integration/mydbsc.Integration.csproj
msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU dbsc.Core.Tests/dbsc.Core.Tests.csproj

nunit-console-x86 "dbsc.Core.Tests/bin/Debug/dbsc.Core.Tests.dll" "msdbsc.Integration/bin/Debug (x86)/msdbsc.Integration.dll" "mongodbsc.Integration/bin/Debug (x86)/mongodbsc.Integration.dll" "pgdbsc.Integration/bin/Debug (x86)/pgdbsc.Integration.dll" "mydbsc.Integration/bin/Debug (x86)/mydbsc.Integration.dll" -nologo -noxml "-out=test_stdout.txt" "-err=test_stderr.txt"