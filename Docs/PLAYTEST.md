# Unity acceptance checks

These checks require Unity 2019.4.41f2 LTS and an activated license. They have **not** been executed in the cloud instance.

1. Import the project using the Built-in Render Pipeline. Confirm the Console has no compilation errors. Run Tools → Cadet Campus → Validate project.
2. Open the Campus scene and enter Play Mode. Confirm the campus appears, the menu responds, and Enter Campus captures the mouse and shows a cadet at the gate.
3. Walk, sprint, jump, approach a wall, and orbit the camera beside a building. Confirm the character stays above the ground, walls block movement, and the camera avoids walls.
4. Approach the turquoise car near the Main Gate and press F. Drive forward, steer both ways, brake, reverse and park. Confirm the suspension settles on all four wheels, control is stable, and displayed speed changes. Press F below 8 km/h and confirm the cadet appears outside the car. Try an exit beside a wall to check clearance.
5. Use M and Escape while moving. Confirm the world and race timer pause, the mouse unlocks, and resuming restores control. Toggle sound from pause.
6. Follow the Academic Block marker and press E. Confirm 100 credits and a Dining Hall objective. Get out of the car to pick up the parcel with E; deliver it on foot to House 3. Confirm the balance reaches 350 and the gate circuit becomes available.
7. Start the circuit from the Main Gate in a car. Drive all six markers in order before the 110-second limit. Confirm the balance becomes 850 and free roam continues. Retry the race. Also let one race expire: it must award no credits and restart from checkpoint one after returning to the gate.
8. Press R on foot and in each car. Confirm safe recovery and continued mission state. Recovery during a race must not reset its timer.
9. Inspect the main locations against the in-game directory. Confirm mission approaches and roads remain unobstructed and pedestrians stay on their intended routes. Court games, swimming and building interiors are outside this prototype's scope.
10. Export a Windows or Linux build, run it outside the Editor, and repeat movement, car entry/driving/exit, interaction, map and pause checks. Inspect the player log for exceptions and shader errors. Share the complete build directory.
