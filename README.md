# Cadet / After Hours

A Unity third-person campus sandbox inspired by the supplied Cadet College map. Walk the campus, borrow a car, deliver a parcel, and beat a timed driving circuit. The campus, characters, cars, UI, and engine sound are created procedurally; no paid assets or GTA assets are needed.

This branch targets **Unity 2019.4.41f2 LTS**. [Download the Unity 2019 project ZIP](https://github.com/penguinytGG121/GAMEaaa/archive/refs/heads/unity-2019-lts.zip). The `main` branch retains the original Unity 2022 project.

## Open and play

1. Use **Unity 2019.4.41f2 LTS** in Unity Hub with an activated Unity license. This project uses the **Built-in Render Pipeline** and legacy Input Manager.
2. In Unity Hub, choose **Add project from disk** and select the folder containing `Assets`, `Packages`, and `ProjectSettings` (this repository's root).
3. Open `Assets/Scenes/Campus.unity`. Press **Play**, then click **Enter Campus** in the Game view.

The scene contains a bootstrap object. The world is generated when Play starts, so an empty-looking Edit Mode scene is expected. Import may create normal Unity settings and cache files. Use a landscape Game view of at least 1280 × 800 for the intended HUD layout.

Fully extract the new ZIP before adding it. Select the actual `GAMEaaa-unity-2019-lts` folder containing `Assets`, `Packages`, and `ProjectSettings`. Some extraction tools create an extra outer folder; open that folder to find the project root. Unity Hub 2019 may label the action **Add** instead of **Add project from disk**. If your installed 2019.4.41f2 Editor is missing from Hub, use **Installs → Locate** to register it first. Use a fresh extraction of this branch for the port.

For an installed Editor, opening the project directly is another option:

```powershell
# Windows PowerShell: replace the project path with your extracted folder.
& 'C:\Program Files\Unity\Hub\Editor\2019.4.41f2\Editor\Unity.exe' -projectPath 'C:\Games\GAMEaaa-unity-2019-lts'
```

The compatibility port keeps the same campus and gameplay. It uses Unity 2019's built-in `Arial.ttf`, the 2019 player-settings and Transform serialization layouts, and C# 7.3 syntax checks. No URP, HDRP, TextMesh Pro, or Input System package is required.

| Control | Action |
| --- | --- |
| WASD / arrow keys | Walk or drive |
| Mouse | Orbit camera |
| Left Shift | Sprint on foot |
| Space | Jump on foot / brake in a car |
| F | Enter nearby car / exit below 8 km/h |
| E | Interact with the gold mission marker |
| M | Open / close campus map; pauses simulation |
| Escape | Pause / resume; releases mouse |
| R | Return to Main Gate / recover current car to its parking spot |

## What's included

- A compact, stylized recreation of the reference's layout: main gate and Bangladesh flag, palm avenue, academic block, dining hall, three houses, mosque, principal's bungalow, faculty quarters, swimming pool, squash court, tennis, basketball, volleyball, football and running track, parade square, and obstacle / ISSB grounds.
- Third-person movement, sprinting, jumping, camera obstruction handling, three cars with WheelCollider suspension, reverse, braking, engine audio, and recovery.
- Twenty animated cadet pedestrians following campus paths and yielding near the player and vehicles.
- Start screen, pause and sound controls, minimap, campus directory map, mission indicators, credits and speedometer.
- A check-in, a pickup and delivery mission, and a repeatable six-checkpoint, 110-second driving circuit.

Follow the gold marker to check in at the Academic Block, collect a parcel at the Dining Hall on foot, and deliver it to House 3. Then take a car to the Main Gate and press E to start the circuit. Timeout allows a retry. Completing the circuit awards 500 credits. Recovery does not stop the timer.

This is an original small prototype with a campus setting. Buildings have exterior collision but no playable interiors. Sports areas are scenery. There are no weapons, police systems, multiplayer, traffic simulation, or saved-game persistence; progress lasts for the current session. Layout proportions and the running-track length are illustrative rather than a survey of the real campus.

## Build a game file

After testing Play Mode, use **Tools → Cadet Campus → Build Windows game** or **Build Linux game**. Install the corresponding Unity build-support module through Unity Hub first. Outputs go to `Builds/Windows` or `Builds/Linux`. Share the **whole output folder**, including the data directory, rather than only the executable.

You can also use File → Build Settings with `Assets/Scenes/Campus.unity` enabled. The Tools menu has scene-opening and project-validation helpers.

For a licensed Linux editor with Linux build support, the batch command is:

```bash
"$UNITY_EDITOR" -batchmode -nographics -quit -projectPath "$PWD" \
  -executeMethod CadetCampus.Editor.CampusTools.BuildLinux -logFile /tmp/campus-build.log
```

This command is provided for Unity-equipped machines; it was **not run in this cloud instance**.

## Cloud development checks

```bash
cd /workspace/GAMEaaa
bash Tools/setup-cloud.sh
bash Tools/verify.sh
```

Setup installs the official .NET 8.0.427 SDK under `/workspace/.campus-tools`, checking its published SHA-512. It downloads a checksum-pinned, NuGet-signed `Unity3D.SDK` 2019.4.15.1 reference package distributed by a community owner. This package contains Unity 2019.4 runtime and editor reference assemblies used to compile-check both game scripts and editor helpers with C# 7.3. The SDK version identifies its 2019.4 API reference baseline; the project targets 2019.4.41f2. Those reference DLLs are not an installed Unity Editor, are not bundled with the game, and cannot run it. NuGet certificate-revocation warnings may appear where CRL destinations cannot be reached; verification is not disabled. Logs remain in `/workspace/.campus-tools/unity2019-reference-verification.log`. Set `CAMPUS_TOOLS_DIR` to use a different location. The .NET SDK is only for cloud checks; you do not need it to play in Unity.

The checks validate Unity asset GUIDs, scene and build wiring, required engine modules, mouse axes, the 2019.4.41f2 version pin, legacy font and settings compatibility, C# syntax and referenced runtime/editor APIs, and mission rules including timeout and replay. Tool outputs stay outside the checkout. No credentials or external game services are required. Downloads use `builds.dotnet.microsoft.com` and `api.nuget.org` through the managed network configuration.

Each cloud task is already isolated. Use the existing checkout; do not create a Git worktree unless requested.

## Validation status

Both runtime and editor C# compilation passed against the 2019.4 reference assemblies with zero warnings/errors and C# 7.3. All 21 mission-rule checks and 66 project structure/compatibility checks passed in this cloud instance.

Unity Editor is not installed in this cloud instance and a Unity license is not supplied. Source validation uses Unity 2019.4 reference assemblies; it does not run the 2019.4.41f2 Editor. **Play Mode, visuals, native vehicle physics, editor execution, Hub import and standalone builds remain unverified here.** The downloadable archive is source, not a tested executable. See [Docs/PLAYTEST.md](Docs/PLAYTEST.md) for the remaining Unity checks.
