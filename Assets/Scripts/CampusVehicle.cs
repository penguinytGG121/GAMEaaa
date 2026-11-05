using UnityEngine;

namespace CadetCampus
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class CampusVehicle : MonoBehaviour
    {
        Rigidbody body;
        readonly WheelCollider[] wheels = new WheelCollider[4];
        readonly Transform[] tireVisuals = new Transform[4];
        readonly Renderer[] brakeLights = new Renderer[2];
        AudioSource engine;
        Vector3 parking;
        Quaternion parkingRotation;
        bool hasDriver, braking;
        float throttle, steer;
        public float SpeedKph { get { return body ? body.velocity.magnitude * 3.6f : 0; } }

        public static CampusVehicle Create(string name, Vector3 position, float heading, Color color)
        {
            var root = new GameObject(name);
            root.layer = 8;
            root.transform.SetPositionAndRotation(position, Quaternion.Euler(0, heading, 0));
            var car = root.AddComponent<CampusVehicle>();
            car.Build(color);
            return car;
        }

        void Build(Color color)
        {
            parking = transform.position;
            parkingRotation = transform.rotation;
            body = GetComponent<Rigidbody>();
            body.mass = 1150;
            body.drag = .08f;
            body.angularDrag = 1.8f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.centerOfMass = new Vector3(0, -.48f, 0);
            var collider = gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, .12f, 0);
            collider.size = new Vector3(1.9f, .7f, 4.2f);
            var paint = CampusWorld.Material("Car paint", color, .6f);
            var glass = CampusWorld.Material("Car windows", new Color(.1f, .19f, .24f), .8f);
            var dark = CampusWorld.Material("Rubber", new Color(.035f, .045f, .05f));
            var chrome = CampusWorld.Material("Wheel alloys", new Color(.65f, .69f, .67f), .75f);
            var lamp = CampusWorld.Material("Headlights", new Color(1, .94f, .65f), .4f);
            var red = CampusWorld.Material("Brake lamps", new Color(.7f, .05f, .03f));
            CampusWorld.Part(transform, "Chassis", PrimitiveType.Cube, Vector3.zero, new Vector3(2, .58f, 4.3f), paint);
            CampusWorld.Part(transform, "Bonnet", PrimitiveType.Cube, new Vector3(0, .37f, 1.28f), new Vector3(1.94f, .2f, 1.5f), paint);
            CampusWorld.Part(transform, "Cabin glass", PrimitiveType.Cube, new Vector3(0, .66f, -.18f), new Vector3(1.74f, .7f, 2.1f), glass);
            CampusWorld.Part(transform, "Roof", PrimitiveType.Cube, new Vector3(0, 1.04f, -.25f), new Vector3(1.85f, .12f, 2.05f), paint);
            var cabinCollider = gameObject.AddComponent<BoxCollider>();
            cabinCollider.center = new Vector3(0, .7f, -.2f);
            cabinCollider.size = new Vector3(1.75f, .7f, 2.1f);
            for (int side = -1; side <= 1; side += 2)
            {
                CampusWorld.Part(transform, "Window pillar", PrimitiveType.Cube, new Vector3(side * .9f, .65f, -.1f), new Vector3(.08f, .75f, .12f), paint);
                CampusWorld.Part(transform, "Mirror", PrimitiveType.Cube, new Vector3(side * 1.08f, .65f, .75f), new Vector3(.24f, .18f, .3f), paint);
                CampusWorld.Part(transform, "Front lamp", PrimitiveType.Cube, new Vector3(side * .67f, .15f, 2.16f), new Vector3(.52f, .22f, .04f), lamp);
                var brake = CampusWorld.Part(transform, "Rear lamp", PrimitiveType.Cube, new Vector3(side * .72f, .15f, -2.16f), new Vector3(.4f, .18f, .04f), red);
                brakeLights[side < 0 ? 0 : 1] = brake.GetComponent<Renderer>();
            }
            CampusWorld.Part(transform, "Front bumper", PrimitiveType.Cube, new Vector3(0, -.13f, 2.18f), new Vector3(1.9f, .14f, .12f), chrome);
            CampusWorld.Part(transform, "Rear bumper", PrimitiveType.Cube, new Vector3(0, -.13f, -2.18f), new Vector3(1.9f, .14f, .12f), chrome);
            CampusWorld.Part(transform, "Grille", PrimitiveType.Cube, new Vector3(0, .1f, 2.18f), new Vector3(.6f, .22f, .03f), dark);
            for (int i = 0; i < 4; i++)
            {
                float side = i % 2 == 0 ? -1 : 1;
                var wheel = new GameObject("Suspension " + i);
                wheel.layer = 8;
                wheel.transform.SetParent(transform, false);
                wheel.transform.localPosition = new Vector3(side * .94f, -.2f, i < 2 ? 1.35f : -1.3f);
                wheels[i] = wheel.AddComponent<WheelCollider>();
                wheels[i].radius = .42f;
                wheels[i].mass = 25;
                wheels[i].suspensionDistance = .22f;
                JointSpring spring = wheels[i].suspensionSpring;
                spring.spring = 38000;
                spring.damper = 4500;
                spring.targetPosition = .5f;
                wheels[i].suspensionSpring = spring;
                wheels[i].forceAppPointDistance = .1f;
                var friction = wheels[i].sidewaysFriction;
                friction.stiffness = 1.5f;
                wheels[i].sidewaysFriction = friction;
                var tire = new GameObject("Tire visual " + i).transform;
                tire.SetParent(transform, false);
                var rubber = CampusWorld.Part(tire, "Tire", PrimitiveType.Cylinder, Vector3.zero, new Vector3(.84f, .14f, .84f), dark);
                rubber.transform.localRotation = Quaternion.Euler(0, 0, 90);
                var rim = CampusWorld.Part(tire, "Rim", PrimitiveType.Cylinder, new Vector3(side * .15f, 0, 0), new Vector3(.48f, .012f, .48f), chrome);
                rim.transform.localRotation = Quaternion.Euler(0, 0, 90);
                tireVisuals[i] = tire;
            }
            engine = gameObject.AddComponent<AudioSource>();
            engine.clip = EngineClip();
            engine.loop = true;
            engine.spatialBlend = 1;
            engine.minDistance = 3;
            engine.maxDistance = 45;
            engine.volume = 0;
            engine.Play();
        }

        static AudioClip EngineClip()
        {
            const int rate = 22050;
            float[] samples = new float[rate];
            for (int i = 0; i < samples.Length; i++)
            {
                float t = (float)i / rate;
                samples[i] = .2f * Mathf.Sin(t * Mathf.PI * 2 * 45) + .1f * Mathf.Sin(t * Mathf.PI * 2 * 90) + .05f * Mathf.Sin(t * Mathf.PI * 2 * 135);
            }
            var clip = AudioClip.Create("Synthesized engine", rate, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        void Update()
        {
            bool input = hasDriver && CampusGame.Instance.AcceptInput;
            throttle = input ? ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) ? 1 : 0)) : 0;
            float turn = input ? ((Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow) ? 1 : 0)) : 0;
            steer = Mathf.MoveTowards(steer, turn, Time.deltaTime * 3);
            braking = !hasDriver || (input && Input.GetKey(KeyCode.Space));
            for (int i = 0; i < wheels.Length; i++)
            {
                Vector3 p;
                Quaternion q;
                wheels[i].GetWorldPose(out p, out q);
                tireVisuals[i].SetPositionAndRotation(p, q);
            }
            engine.volume = hasDriver ? .15f : 0;
            engine.pitch = .75f + SpeedKph / 65;
            foreach (var lightRenderer in brakeLights)
                lightRenderer.material.color = braking ? new Color(1, .12f, .05f) : new Color(.55f, .03f, .02f);
        }

        void FixedUpdate()
        {
            float speed = Vector3.Dot(body.velocity, transform.forward);
            bool reverseBraking = throttle != 0 && speed * throttle < -1;
            float torque = reverseBraking || braking ? 0 : throttle * 1400;
            if ((speed > 24 && throttle > 0) || (speed < -9 && throttle < 0)) torque = 0;
            float steering = steer * Mathf.Lerp(32, 12, Mathf.InverseLerp(0, 24, Mathf.Abs(speed)));
            for (int i = 0; i < 4; i++)
            {
                wheels[i].steerAngle = i < 2 ? steering : 0;
                wheels[i].motorTorque = i >= 2 ? torque : 0;
                wheels[i].brakeTorque = braking ? 2800 : reverseBraking ? 2200 : throttle == 0 ? 70 : 0;
            }
            body.AddForce(-transform.up * body.velocity.sqrMagnitude * 4);
            if (transform.position.y < -8) Recover();
        }

        public void SetDriver(bool active) { hasDriver = active; }

        public bool TryGetExit(out Vector3 point)
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 candidate = transform.position + transform.right * (side * 2.2f);
                RaycastHit floor;
                if (!Physics.Raycast(candidate + Vector3.up * 3, Vector3.down, out floor, 6, 1 << 0)) continue;
                candidate = floor.point + Vector3.up * .08f;
                // Ignore triggers and the hidden player; require standing headroom.
                if (Physics.CheckCapsule(candidate + Vector3.up * .38f, candidate + Vector3.up * 1.6f, .34f, (1 << 0) | (1 << 8), QueryTriggerInteraction.Ignore)) continue;
                point = candidate;
                return true;
            }
            point = Vector3.zero;
            return false;
        }

        public void Recover()
        {
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            transform.SetPositionAndRotation(parking, parkingRotation);
            body.position = parking;
            body.rotation = parkingRotation;
        }

        void OnDestroy()
        {
            if (engine && engine.clip) Destroy(engine.clip);
        }
    }
}
