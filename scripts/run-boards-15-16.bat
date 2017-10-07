SETLOCAL
pushd %~dp0..
dotnet run -c Release --no-build --no-restore -- 15
dotnet run -c Release --no-build --no-restore -- 16
::dotnet run -c Release --no-build --no-restore -- 17
::dotnet run -c Release --no-build --no-restore -- 18
::pause