#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
tools_dir="${CAMPUS_TOOLS_DIR:-/workspace/.campus-tools}"
if [[ ! -x "$tools_dir/dotnet/dotnet" ]]; then
    echo 'First run bash Tools/setup-cloud.sh.' >&2
    exit 1
fi
tools_dir="$(cd "$tools_dir" && pwd)"
export DOTNET_CLI_HOME="$tools_dir/cache"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
cd "$project_dir"
python3 Tools/check-project.py
"$tools_dir/dotnet/dotnet" build Tools/Campus.Compile.csproj --nologo \
    "-p:UnityReferenceDir=$tools_dir/unity-reference/lib/netstandard2.0" \
    "-p:BaseIntermediateOutputPath=$tools_dir/obj/compile/" \
    "-p:BaseOutputPath=$tools_dir/bin/compile/"
"$tools_dir/dotnet/dotnet" build Tools/Campus.RulesTests.csproj --nologo \
    "-p:BaseIntermediateOutputPath=$tools_dir/obj/rules/" \
    "-p:BaseOutputPath=$tools_dir/bin/rules/"
"$tools_dir/dotnet/dotnet" "$tools_dir/bin/rules/Debug/net8.0/Campus.RulesTests.dll"
echo 'Source checks complete. Play Mode, rendering, physics, editor helpers and standalone export require Unity validation.'
