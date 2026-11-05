#!/usr/bin/env python3
"""Check Unity file wiring without pretending to run the engine."""
import json
import pathlib
import re
import sys

root = pathlib.Path(__file__).resolve().parents[1]
errors = []
checks = 0

def check(condition, message):
    global checks
    checks += 1
    if not condition:
        errors.append(message)

manifest = json.loads((root / 'Packages/manifest.json').read_text())['dependencies']
for module in ('audio', 'imgui', 'physics', 'textrendering', 'vehicles'):
    check(manifest.get('com.unity.modules.' + module) == '1.0.0', 'missing required module: ' + module)

guids = {}
for meta in (root / 'Assets').rglob('*.meta'):
    match = re.search(r'^guid: ([a-f0-9]{32})$', meta.read_text(), re.M)
    check(match is not None, 'invalid guid in ' + str(meta.relative_to(root)))
    if match:
        guid = match.group(1)
        check(guid not in guids, 'duplicate guid: ' + guid)
        guids[guid] = meta.with_suffix('')
    check(meta.with_suffix('').exists(), 'orphan .meta: ' + str(meta))

for asset in (root / 'Assets').rglob('*'):
    if asset.is_file() and asset.suffix != '.meta':
        check(pathlib.Path(str(asset) + '.meta').exists(), 'asset missing .meta: ' + str(asset))

scene = (root / 'Assets/Scenes/Campus.unity').read_text()
scripts = re.findall(r'm_Script: \{fileID: 11500000, guid: ([a-f0-9]{32}), type: 3\}', scene)
check(len(scripts) == 1, 'scene must have one bootstrap script')
for guid in scripts:
    check(guid in guids and guids[guid].name == 'CampusGame.cs', 'scene bootstrap script is not wired to CampusGame.cs')
build = (root / 'ProjectSettings/EditorBuildSettings.asset').read_text()
check('path: Assets/Scenes/Campus.unity' in build and 'enabled: 1' in build, 'campus not enabled for builds')
scene_guid = re.search(r'^guid: ([a-f0-9]{32})$', pathlib.Path(str(root / 'Assets/Scenes/Campus.unity') + '.meta').read_text(), re.M).group(1)
check('guid: ' + scene_guid in build, 'build settings refer to wrong scene guid')
inputs = (root / 'ProjectSettings/InputManager.asset').read_text()
check('m_Name: Mouse X' in inputs and 'm_Name: Mouse Y' in inputs, 'mouse input axes are missing')
settings = (root / 'ProjectSettings/ProjectSettings.asset').read_text()
check('activeInputHandler: 0' in settings, 'legacy input backend is not selected')
version = (root / 'ProjectSettings/ProjectVersion.txt').read_text()
check(version.strip() == 'm_EditorVersion: 2019.4.41f2', 'project must target Unity 2019.4.41f2')
check('serializedVersion: 20' in settings, 'player settings must use the Unity 2019 serialization version')
world = (root / 'Assets/Scripts/CampusWorld.cs').read_text()
check('GetBuiltinResource<Font>("Arial.ttf")' in world and 'LegacyRuntime.ttf' not in world,
      'Unity 2019 needs its built-in Arial font')
for project in ('Campus.Compile.csproj', 'Campus.EditorCompile.csproj', 'Campus.RulesTests.csproj'):
    check('<LangVersion>7.3</LangVersion>' in (root / 'Tools' / project).read_text(),
          'C# checks must use Unity 2019-compatible syntax: ' + project)

if errors:
    print('\n'.join('FAIL: ' + e for e in errors), file=sys.stderr)
    sys.exit(1)
print(f'{checks} project structure checks passed (no Unity execution).')
