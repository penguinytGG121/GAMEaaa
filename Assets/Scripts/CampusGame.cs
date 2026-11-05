using UnityEngine;

namespace CadetCampus
{
    public sealed class CampusGame : MonoBehaviour
    {
        public static CampusGame Instance { get; private set; }
        public CampusWorld World { get; private set; }
        public CampusPlayer Player { get; private set; }
        public CampusCamera FollowCamera { get; private set; }
        public CampusVehicle ActiveCar { get; private set; }
        public CampusRules Rules { get; private set; }
        public bool Started { get; private set; }
        public bool Paused { get; private set; }
        public bool MapOpen { get; private set; }
        public bool Muted { get; private set; }
        public string Toast { get; private set; }
        public float ToastUntil { get; private set; }
        public bool AcceptInput { get { return Started && !Paused && !MapOpen; } }
        public Vector3 Position { get { return ActiveCar ? ActiveCar.transform.position : Player.transform.position; } }
        public Transform Focus { get { return ActiveCar ? ActiveCar.transform : Player.transform; } }
        public float Heading { get { return Focus.eulerAngles.y; } }
        public CampusVehicle[] Cars { get; private set; }
        Transform marker;

        public static readonly Vector3[] Circuit =
        {
            new Vector3(-55, 0, -125), new Vector3(-55, 0, 64),
            new Vector3(99, 0, 64), new Vector3(99, 0, -125),
            new Vector3(0, 0, -125), new Vector3(0, 0, -143)
        };

        void Awake()
        {
            Instance = this;
            Rules = new CampusRules();
            Application.targetFrameRate = 60;
            QualitySettings.shadowDistance = 95;
            QualitySettings.shadows = ShadowQuality.All;
            Physics.gravity = new Vector3(0, -22, 0);
            World = new GameObject("Campus environment").AddComponent<CampusWorld>();
            World.Build();
            Player = new GameObject("Cadet").AddComponent<CampusPlayer>();
            Player.Initialize(new Vector3(3, 0.15f, -143));
            Cars = new CampusVehicle[3];
            Cars[0] = CampusVehicle.Create("Campus cruiser", new Vector3(-5, 0.85f, -137), 0, new Color(.1f, .65f, .62f));
            Cars[1] = CampusVehicle.Create("House runabout", new Vector3(-55, .85f, -67), 0, new Color(.82f, .37f, .18f));
            Cars[2] = CampusVehicle.Create("Faculty sedan", new Vector3(99, .85f, -93), 180, new Color(.78f, .76f, .58f));
            FollowCamera = new GameObject("Campus camera").AddComponent<CampusCamera>();
            FollowCamera.Initialize(this);
            gameObject.AddComponent<CampusHud>();
            marker = World.CreateObjectiveMarker();
            SpawnPedestrians();
            marker.position = Objective;
            AudioListener.pause = true;
            SetCursor(false);
        }

        void SpawnPedestrians()
        {
            Vector3[][] paths =
            {
                new [] { new Vector3(-47,0,-30), new Vector3(83,0,-30), new Vector3(83,0,56), new Vector3(-47,0,56) },
                new [] { new Vector3(-62,0,-117), new Vector3(-62,0,53), new Vector3(-177,0,53), new Vector3(-177,0,-117) },
                new [] { new Vector3(110,0,-134), new Vector3(110,0,54), new Vector3(177,0,54), new Vector3(177,0,-134) },
                new [] { new Vector3(12,0,-117), new Vector3(12,0,-49), new Vector3(22,0,-49), new Vector3(22,0,-117) }
            };
            for (int i = 0; i < 20; i++)
            {
                var npc = new GameObject("Cadet pedestrian " + (i + 1)).AddComponent<CampusPedestrian>();
                npc.Initialize(paths[i % paths.Length], i);
            }
        }

        public void Begin()
        {
            Started = true;
            Paused = false;
            Time.timeScale = 1;
            AudioListener.pause = false;
            SetCursor(true);
            Notify("Welcome to campus. Head to the gold marker at the Academic Block.");
        }

        void Update()
        {
            if (!Started) return;
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (MapOpen) MapOpen = false;
                else Paused = !Paused;
                ApplyPause();
            }
            if (!Paused && Input.GetKeyDown(KeyCode.M))
            {
                MapOpen = !MapOpen;
                ApplyPause();
            }
            if (!AcceptInput) return;
            if (Input.GetKeyDown(KeyCode.F)) ToggleCar();
            if (Input.GetKeyDown(KeyCode.R)) ResetActor();
            if (Input.GetKeyDown(KeyCode.E) && !Rules.CircuitRunning)
            {
                bool changed = Rules.Interact(NearObjective, ActiveCar != null);
                if (changed) Notify(Rules.Stage == MissionStage.Circuit ? "Campus circuit started. Follow the gold gates!" : "Objective complete. Follow your next marker.");
            }
            if (Rules.Tick(Time.deltaTime)) Notify("Time is up. Return to the Main Gate to try the circuit again.");
            if (Rules.CircuitRunning && ActiveCar && NearObjective)
            {
                Rules.ReachCheckpoint(true);
                Notify(Rules.Stage == MissionStage.Complete ? "Circuit complete! +500 credits. Free roam, or return to race again." : "Checkpoint " + Rules.Checkpoint + "/6 cleared");
            }
            Vector3 p = Objective;
            marker.position = p + Vector3.up * (.2f + Mathf.Sin(Time.time * 2) * .12f);
            marker.localRotation = Quaternion.Euler(0, Time.time * 28, 0);
            marker.localScale = Rules.CircuitRunning ? new Vector3(3.4f, 1.1f, 3.4f) : Vector3.one;
        }

        public void Resume()
        {
            Paused = false;
            MapOpen = false;
            ApplyPause();
        }

        void ApplyPause()
        {
            Time.timeScale = Paused || MapOpen ? 0 : 1;
            AudioListener.pause = Paused || MapOpen;
            SetCursor(AcceptInput);
        }

        static void SetCursor(bool locked)
        {
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !locked;
        }

        public void ToggleMute()
        {
            Muted = !Muted;
            AudioListener.volume = Muted ? 0 : 1;
        }

        void ToggleCar()
        {
            if (ActiveCar)
            {
                if (ActiveCar.SpeedKph > 8) { Notify("Slow down to less than 8 km/h to exit."); return; }
                Vector3 exit;
                if (!ActiveCar.TryGetExit(out exit)) { Notify("Both doors are blocked. Move the car to an open space."); return; }
                var previous = ActiveCar;
                ActiveCar = null;
                previous.SetDriver(false);
                Player.ExitVehicle(exit, previous.transform.eulerAngles.y);
                return;
            }
            var nearest = NearestCar;
            if (!nearest) { Notify("Get closer to a parked car, then press F."); return; }
            ActiveCar = nearest;
            Player.EnterVehicle();
            nearest.SetDriver(true);
            Notify("WASD drive  /  Space handbrake  /  F exit  /  R recover");
        }

        public CampusVehicle NearestCar
        {
            get
            {
                CampusVehicle result = null;
                float distance = 4.8f;
                foreach (var car in Cars)
                {
                    float d = Vector3.Distance(Player.transform.position, car.transform.position);
                    if (d < distance) { distance = d; result = car; }
                }
                return result;
            }
        }

        public Vector3 Objective
        {
            get
            {
                switch (Rules.Stage)
                {
                    case MissionStage.Orientation: return new Vector3(12, 0, -28);
                    case MissionStage.Collect: return new Vector3(12, 0, 27);
                    case MissionStage.Deliver: return new Vector3(-133, 0, -9);
                    case MissionStage.Circuit: return Circuit[Mathf.Clamp(Rules.Checkpoint, 0, Circuit.Length - 1)];
                    default: return new Vector3(0, 0, -143);
                }
            }
        }

        public bool NearObjective { get { return Vector2.Distance(new Vector2(Position.x, Position.z), new Vector2(Objective.x, Objective.z)) < (Rules.CircuitRunning ? 10 : 6); } }

        public string ObjectiveTitle
        {
            get
            {
                switch (Rules.Stage)
                {
                    case MissionStage.Orientation: return "01  /  CHECK IN";
                    case MissionStage.Collect: return "02  /  THE DINING HALL RUN";
                    case MissionStage.Deliver: return "03  /  SPECIAL DELIVERY";
                    case MissionStage.FindCar: return "04  /  CAMPUS CIRCUIT";
                    case MissionStage.Circuit: return "CIRCUIT  /  " + (Rules.Checkpoint + 1) + " OF 6";
                    default: return "CAMPUS IS YOURS";
                }
            }
        }

        public string ObjectiveDetail
        {
            get
            {
                switch (Rules.Stage)
                {
                    case MissionStage.Orientation: return "Meet at the Academic Block entrance. Press E to check in.";
                    case MissionStage.Collect: return "Collect the supply parcel outside the Dining Hall. E on foot.";
                    case MissionStage.Deliver: return "Deliver the parcel to House 3. E on foot.";
                    case MissionStage.FindCar: return "Take a car to the Main Gate marker. Press E to start.";
                    case MissionStage.Circuit: return "Drive through the gold markers before time runs out.";
                    default: return "Explore freely. Return to the Main Gate in a car and press E to race again.";
                }
            }
        }

        void ResetActor()
        {
            if (ActiveCar) { ActiveCar.Recover(); Notify("Car recovered to its parking spot. Circuit timer continues."); }
            else { Player.Teleport(new Vector3(3, .2f, -143)); Notify("Returned to the Main Gate."); }
        }

        public void Notify(string message)
        {
            Toast = message;
            ToastUntil = Time.unscaledTime + 5;
        }

        void OnApplicationFocus(bool focus)
        {
            if (!focus && Started && !Paused) { Paused = true; ApplyPause(); }
        }

        void OnDestroy()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            SetCursor(false);
            if (Instance == this) Instance = null;
        }
    }
}
