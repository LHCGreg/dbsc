msbuild /t:Build /p:Configuration=Debug;Platform=x86 msdbsc.Integration.csproj
nunit-console-x86 "bin/Debug (x86)/msdbsc.Integration.dll" -nologo -noxml "-out=bin/Debug (x86)/test_stdout.txt" "-err=bin/Debug (x86)/test_stderr.txt"