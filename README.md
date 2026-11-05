# Cadet / After Hours

A Unity third-person campus sandbox inspired by the supplied Cadet College map. Walk the campus, borrow a car, deliver a parcel, and beat a timed driving circuit. The campus, characters, cars, UI, and engine sound are created procedurally; no paid assets or GTA assets are needed.

## Open and play

1. Install **Unity 2022.3.62f1 LTS** through Unity Hub and activate your Unity license. This project uses the **Built-in Render Pipeline** and legacy Input Manager.
2. In Unity Hub, choose **Add project from disk** and select the folder containing `Assets`, `Packages`, and `ProjectSettings` (this repository's root).
3. Open `Assets/Scenes/Campus.unity`. Press **Play**, then click **Enter Campus** in the Game view.

The scene contains a bootstrap object. The world is generated when Play starts, so an empty-looking Edit Mode scene is expected. Import may create normal Unity settings and cache files. Use a landscape Game view of at least 1280 × 800 for the intended HUD layout.

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

Setup installs the official .NET 8.0.427 SDK under `/workspace/.campus-tools`, checking its published SHA-512. It downloads a checksum-pinned, NuGet-signed `UnityEngine.Modules` 2021.3.33 reference package distributed by a community owner, solely to compile-check the runtime scripts. Those reference DLLs are not Unity Editor, are not bundled with the game, and cannot run it. NuGet certificate-revocation warnings may appear where CRL destinations cannot be reached; verification is not disabled. The logs remain in the tools directory. Set `CAMPUS_TOOLS_DIR` to use a different location.

The checks validate Unity asset GUIDs, scene and build wiring, required engine modules, mouse axes, C# syntax and referenced APIs, and mission rules including timeout and replay. Tool outputs stay outside the checkout. No credentials or external game services are required. Downloads use `builds.dotnet.microsoft.com` and `api.nuget.org` through the managed network configuration.

Each cloud task is already isolated. Use the existing checkout; do not create a Git worktree unless requested.

## Validation status

In this cloud instance, runtime C# compilation against the references passed with zero warnings/errors, all 21 mission-rule checks passed, and Unity project structure checks passed. Unity Editor is not installed and a Unity license is not supplied, so **Play Mode, visuals, native vehicle physics, editor helper compilation, and standalone builds remain unverified**. The downloadable archive is source, not a tested executable. See [Docs/PLAYTEST.md](Docs/PLAYTEST.md) for the remaining Unity checks.
