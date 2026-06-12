$ErrorActionPreference = 'Stop'
dotnet ef database update
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
dotnet run --no-launch-profile --urls http://127.0.0.1:5237
