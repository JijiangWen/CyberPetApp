#!/usr/bin/env bash
set -euo pipefail
dotnet ef database update
dotnet run --no-launch-profile --urls http://127.0.0.1:5237
