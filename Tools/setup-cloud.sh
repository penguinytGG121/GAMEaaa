#!/usr/bin/env bash
set -euo pipefail

# Compile-check tooling only. Unity Hub/Editor and an activated license are separate.
tools_dir="${CAMPUS_TOOLS_DIR:-/workspace/.campus-tools}"
mkdir -p "$tools_dir"
tools_dir="$(cd "$tools_dir" && pwd)"
export DOTNET_CLI_HOME="$tools_dir/cache"
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_GENERATE_ASPNET_CERTIFICATE=false

if [[ "$(uname -s)" != Linux || "$(uname -m)" != x86_64 ]]; then
    echo 'Cloud bootstrap requires Linux x86_64. For local play, open the project in Unity Hub.' >&2
    exit 1
fi

sdk_version=8.0.427
sdk_hash=85725692f465f5024b9b42a159a29d9f73e8c57ad9e8bfe1e7028a505d18031d68931e8aa3f9de44145b08170dde897c8a78e1aa18d4219698b6891af7095d39
if [[ ! -x "$tools_dir/dotnet/dotnet" ]] || [[ "$("$tools_dir/dotnet/dotnet" --version)" != "$sdk_version" ]]; then
    curl -fsSL --retry 2 --max-time 300 \
        "https://builds.dotnet.microsoft.com/dotnet/Sdk/$sdk_version/dotnet-sdk-$sdk_version-linux-x64.tar.gz" \
        -o "$tools_dir/sdk.tar.gz"
    printf '%s  %s\n' "$sdk_hash" "$tools_dir/sdk.tar.gz" | sha512sum -c -
    mkdir -p "$tools_dir/dotnet"
    tar -xzf "$tools_dir/sdk.tar.gz" -C "$tools_dir/dotnet"
fi

package="$tools_dir/unityengine.modules.nupkg"
reference_hash=69dede070906eba450d13a00303fda93c319ea97e0ad85f170f19fb4df07ef27649f94ddaf0954e6a8194e41e98ce198a0826f224dfeefb556ab9dff514d9da8
if [[ ! -f "$package" ]]; then
    curl -fsSL --retry 2 --max-time 90 \
        https://api.nuget.org/v3-flatcontainer/unityengine.modules/2021.3.33/unityengine.modules.2021.3.33.nupkg \
        -o "$package"
fi
printf '%s  %s\n' "$reference_hash" "$package" | sha512sum -c -
"$tools_dir/dotnet/dotnet" nuget verify "$package" --all --verbosity normal \
    > "$tools_dir/unity-reference-verification.log" 2>&1 || {
        cat "$tools_dir/unity-reference-verification.log" >&2
        exit 1
    }
python3 - "$package" "$tools_dir/unity-reference" <<'PY'
import pathlib
import sys
import zipfile
with zipfile.ZipFile(sys.argv[1]) as package:
    destination = pathlib.Path(sys.argv[2]).resolve()
    for item in package.infolist():
        resolved = (destination / item.filename).resolve()
        if not resolved.is_relative_to(destination):
            raise ValueError('Unsafe package member')
    package.extractall(destination)
PY

echo 'C# tooling installed. Unity reference package is compile-only, distributed by a community NuGet owner.'
if rg -q 'warn :' "$tools_dir/unity-reference-verification.log"; then
    echo "NuGet verification warnings (see $tools_dir/unity-reference-verification.log):"
    rg 'warn :' "$tools_dir/unity-reference-verification.log"
fi
echo 'No Unity Editor, license, native engine, or player build was installed by this script.'
