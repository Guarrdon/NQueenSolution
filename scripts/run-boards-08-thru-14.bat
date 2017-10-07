SETLOCAL
pushd %~dp0..
dotnet run -c Release --no-build --no-restore -- 8
dotnet run -c Release --no-build --no-restore -- 10
dotnet run -c Release --no-build --no-restore -- 12
dotnet run -c Release --no-build --no-restore -- 13
dotnet run -c Release --no-build --no-restore -- 14
::pause