msbuild /t:Build /p:Configuration=Debug;Platform=AnyCPU dbsc.Core.Tests.csproj
nunit-console "bin/Debug/dbsc.Core.Tests.dll" -nologo -noxml "-out=bin/Debug_AnyCPU/test_stdout.txt" "-err=bin/Debug/test_stderr.txt"