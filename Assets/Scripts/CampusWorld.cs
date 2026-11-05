using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CadetCampus
{
    public sealed class CampusWorld : MonoBehaviour
    {
        public sealed class Landmark
        {
            public string Name;
            public Vector2 Position;
            public Vector2 Size;
            public Color Color;
            public Landmark(string name, float x, float z, float width, float depth, Color color)
            { Name = name; Position = new Vector2(x, z); Size = new Vector2(width, depth); Color = color; }
        }

        public readonly List<Landmark> Landmarks = new List<Landmark>();
        public readonly List<Rect> Roads = new List<Rect>();
        readonly List<Rect> reserved = new List<Rect>();
        static readonly Dictionary<string, UnityEngine.Material> materials = new Dictionary<string, UnityEngine.Material>();
        Font signFont;
        UnityEngine.Material grass, cream, trim, roof, asphalt, pavement, white, dark, glass, bark, foliage, gold;

        public static UnityEngine.Material Material(string name, Color color, float smoothness = .1f)
        {
            string key = name + ColorUtility.ToHtmlStringRGBA(color) + smoothness;
            UnityEngine.Material material;
            if (materials.TryGetValue(key, out material) && material) return material;
            material = new UnityEngine.Material(Shader.Find("Standard"));
            material.name = name;
            material.color = color;
            material.SetFloat("_Glossiness", smoothness);
            materials[key] = material;
            return material;
        }

        public static GameObject Part(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, UnityEngine.Material material, bool solid = false)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.layer = parent ? parent.gameObject.layer : 0;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid)
            {
                var collider = part.GetComponent<Collider>();
                collider.enabled = false;
                Destroy(collider);
            }
            return part;
        }

        GameObject Box(string name, Vector3 p, Vector3 size, UnityEngine.Material material, bool solid = false)
        { return Part(transform, name, PrimitiveType.Cube, p, size, material, solid); }

        public void Build()
        {
            grass = Material("Meadow", new Color(.32f, .44f, .21f));
            cream = Material("Warm plaster", new Color(.82f, .76f, .59f));
            trim = Material("Sandstone trim", new Color(.95f, .9f, .73f));
            roof = Material("Terracotta roofs", new Color(.57f, .24f, .15f));
            asphalt = Material("Asphalt", new Color(.18f, .21f, .22f));
            pavement = Material("Walkways", new Color(.62f, .61f, .51f));
            white = Material("Court markings", new Color(.94f, .92f, .8f));
            dark = Material("Ironwork", new Color(.08f, .15f, .15f));
            glass = Material("Blue window glass", new Color(.14f, .3f, .31f), .65f);
            bark = Material("Tree trunks", new Color(.3f, .23f, .13f));
            foliage = Material("Tree canopy", new Color(.27f, .41f, .13f));
            gold = Material("Objective gold", new Color(1, .73f, .18f), .3f);
            Lighting();
            Box("Campus lawn", new Vector3(0, -.5f, 0), new Vector3(600, 1, 560), grass, true);
            Road(0, -104, 15, 142);
            Road(-55, 20, 11, 305);
            Road(99, -31, 11, 200);
            Road(12, -42, 176, 11);
            Road(0, 64, 363, 11);
            Road(0, -125, 365, 12);
            // Paths along the central avenue.
            Box("East avenue path", new Vector3(13, .055f, -91), new Vector3(4, .11f, 96), pavement);
            Box("West avenue path", new Vector3(-13, .055f, -91), new Vector3(4, .11f, 96), pavement);
            Building("Academic Block", 12, -7, 111, 27, 2, true);
            Building("Dining Hall", 12, 42, 103, 22, 1, true);
            Building("House 3", -133, 7, 49, 25, 2, false);
            Building("House 2", -139, -49, 44, 24, 2, false);
            Building("House 1", -90, -49, 43, 24, 2, false);
            Building("Principal's Bungalow", -124, -102, 57, 23, 1, false);
            Building("Faculty Quarters", 143, -105, 61, 23, 3, false);
            Building("Squash Court", 143, -5, 54, 23, 1, false);
            Mosque(-83, 9);
            Athletics();
            Courts();
            Pool(143, 34);
            DrillGround();
            ObstacleGround();
            MainGate();
            Boundary();
            Gardens();
            StreetFurniture();
            Crosswalk(0, -45, false);
            Crosswalk(-55, -8, true);
            Crosswalk(99, 34, true);
            Crosswalk(0, -125, false);
        }

        void Lighting()
        {
            var sun = new GameObject("Late afternoon sun").AddComponent<Light>();
            sun.transform.SetParent(transform, false);
            sun.type = LightType.Directional;
            sun.color = new Color(1, .89f, .7f);
            sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = .65f;
            sun.transform.rotation = Quaternion.Euler(43, -38, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.58f, .69f, .76f);
            RenderSettings.ambientEquatorColor = new Color(.55f, .58f, .47f);
            RenderSettings.ambientGroundColor = new Color(.26f, .3f, .19f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.66f, .78f, .76f);
            RenderSettings.fogDensity = .0018f;
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader)
            {
                var sky = new UnityEngine.Material(skyShader);
                sky.SetColor("_SkyTint", new Color(.58f, .66f, .73f));
                sky.SetFloat("_AtmosphereThickness", .8f);
                sky.SetFloat("_SunSize", .035f);
                RenderSettings.skybox = sky;
            }
        }

        void Road(float x, float z, float width, float depth)
        {
            Box("Road shoulders", new Vector3(x, .012f, z), new Vector3(width + 3, .024f, depth + 3), pavement);
            Box("Campus road", new Vector3(x, .034f, z), new Vector3(width, .045f, depth), asphalt);
            Roads.Add(new Rect(x - width * .5f, z - depth * .5f, width, depth));
            reserved.Add(new Rect(x - width * .5f - 5, z - depth * .5f - 5, width + 10, depth + 10));
            bool vertical = depth > width;
            int count = Mathf.FloorToInt((vertical ? depth : width) / 9);
            for (int i = 0; i < count; i++)
            {
                float along = (i + .5f) * 9 - (vertical ? depth : width) * .5f;
                Box("Lane dash", new Vector3(x + (vertical ? 0 : along), .067f, z + (vertical ? along : 0)), vertical ? new Vector3(.14f, .015f, 3) : new Vector3(3, .015f, .14f), white);
            }
        }

        void Crosswalk(float x, float z, bool alongZ)
        {
            for (int i = -4; i <= 4; i++)
                Box("Pedestrian crossing", new Vector3(x + (alongZ ? i : 0), .08f, z + (alongZ ? 0 : i)), alongZ ? new Vector3(.55f, .015f, 4) : new Vector3(4, .015f, .55f), white);
        }

        void Reserve(string name, float x, float z, float w, float d, Color color)
        {
            Landmarks.Add(new Landmark(name, x, z, w, d, color));
            reserved.Add(new Rect(x - w * .5f - 4, z - d * .5f - 4, w + 8, d + 8));
        }

        void Building(string name, float x, float z, float width, float depth, int levels, bool central)
        {
            Reserve(name, x, z, width, depth, new Color(.77f, .48f, .32f));
            float height = levels * 3.5f;
            Box(name + " foundation", new Vector3(x, .14f, z), new Vector3(width + 3, .28f, depth + 3), trim, true);
            Box(name + " plaster walls", new Vector3(x, height * .5f + .28f, z), new Vector3(width, height, depth), cream, true);
            for (int floor = 0; floor < levels; floor++)
            {
                float y = floor * 3.5f + 2;
                Box("Floor band", new Vector3(x, floor * 3.5f + .6f, z), new Vector3(width + .2f, .2f, depth + .2f), trim);
                int windows = Mathf.FloorToInt(width / 5);
                for (int i = 0; i < windows; i++)
                {
                    float wx = x - width / 2 + (i + .5f) * width / windows;
                    if (Mathf.Abs(wx - x) < 4 && floor == 0) continue;
                    Window(wx, y, z - depth / 2 - .07f, false);
                    Window(wx, y, z + depth / 2 + .07f, false);
                }
                for (int i = 0; i < 4; i++)
                {
                    float wz = z - depth / 2 + (i + .5f) * depth / 4;
                    Window(x - width / 2 - .07f, y, wz, true);
                    Window(x + width / 2 + .07f, y, wz, true);
                }
            }
            Roof(x, height + .3f, z, width + 3, depth + 3, central ? 3.8f : 3);
            float front = z - depth / 2;
            Box("Entrance door", new Vector3(x, 1.55f, front - .1f), new Vector3(3.4f, 2.6f, .15f), dark);
            float canopyWidth = central ? 16 : 9;
            Box("Entry paving", new Vector3(x, .09f, front - 4), new Vector3(canopyWidth + 2, .18f, 8), pavement);
            for (int side = -1; side <= 1; side += 2)
                Part(transform, "Portico column", PrimitiveType.Cylinder, new Vector3(x + side * (canopyWidth / 2 - 1), 2, front - 3), new Vector3(.7f, 2, .7f), trim, true);
            Box("Portico lintel", new Vector3(x, 4.1f, front - 2.5f), new Vector3(canopyWidth, .4f, 5), trim);
            Roof(x, 4.3f, front - 2.5f, canopyWidth + .8f, 6, 1.4f);
            Sign(name.ToUpperInvariant(), new Vector3(x, 3.7f, front - 5.6f), Mathf.Min(width - 3, 27), .65f);
        }

        void Window(float x, float y, float z, bool side)
        {
            Box("Window frame", new Vector3(x, y, z), side ? new Vector3(.12f, 1.9f, 1.7f) : new Vector3(1.7f, 1.9f, .12f), trim);
            Box("Window glass", new Vector3(x + (side ? (x > 0 ? .075f : -.075f) : 0), y, z + (!side ? (z > 0 ? .075f : -.075f) : 0)), side ? new Vector3(.14f, 1.6f, 1.4f) : new Vector3(1.4f, 1.6f, .14f), glass);
        }

        void Roof(float x, float y, float z, float width, float depth, float height)
        {
            float w = width * .5f, d = depth * .5f, ridge = Mathf.Max(0, w - d * .7f);
            Vector3[] v =
            {
                new Vector3(-w,0,-d),new Vector3(w,0,-d),new Vector3(ridge,height,0),new Vector3(-ridge,height,0),
                new Vector3(w,0,-d),new Vector3(w,0,d),new Vector3(ridge,height,0),
                new Vector3(w,0,d),new Vector3(-w,0,d),new Vector3(-ridge,height,0),new Vector3(ridge,height,0),
                new Vector3(-w,0,d),new Vector3(-w,0,-d),new Vector3(-ridge,height,0)
            };
            Mesh mesh = new Mesh { name = "Hipped terracotta roof", vertices = v, triangles = new [] { 0,3,2,0,2,1,4,6,5,7,10,9,7,9,8,11,13,12 } };
            mesh.RecalculateNormals();
            var go = new GameObject("Hipped roof");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(x, y, z);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = roof;
        }

        void Sign(string title, Vector3 position, float width = 20, float size = .7f)
        {
            Box("Sign backing", position + Vector3.forward * .06f, new Vector3(width, 1.7f, .2f), dark);
            var sign = new GameObject(title).AddComponent<TextMesh>();
            sign.transform.SetParent(transform, false);
            sign.transform.localPosition = position + Vector3.back * .08f;
            sign.text = title;
            sign.anchor = TextAnchor.MiddleCenter;
            sign.alignment = TextAlignment.Center;
            sign.fontSize = 48;
            if (!signFont) signFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            sign.font = signFont;
            sign.GetComponent<MeshRenderer>().sharedMaterial = signFont.material;
            sign.characterSize = size;
            sign.color = new Color(.98f, .9f, .67f);
        }

        void Mosque(float x, float z)
        {
            Reserve("Mosque", x, z, 24, 24, new Color(.43f, .64f, .53f));
            Box("Mosque courtyard", new Vector3(x, .08f, z), new Vector3(31, .16f, 31), pavement);
            Box("Mosque", new Vector3(x, 3, z), new Vector3(22, 6, 20), trim, true);
            Part(transform, "Dome drum", PrimitiveType.Cylinder, new Vector3(x, 6.2f, z), new Vector3(14, .7f, 14), cream);
            Part(transform, "Mosque dome", PrimitiveType.Sphere, new Vector3(x, 7.3f, z), new Vector3(14, 9, 14), trim);
            Part(transform, "Dome finial", PrimitiveType.Cylinder, new Vector3(x, 12.6f, z), new Vector3(.25f, 1.2f, .25f), gold);
            Part(transform, "Minaret", PrimitiveType.Cylinder, new Vector3(x + 13, 8, z + 5), new Vector3(2.4f, 8, 2.4f), cream, true);
            Part(transform, "Minaret balcony", PrimitiveType.Cylinder, new Vector3(x + 13, 13, z + 5), new Vector3(4, .3f, 4), trim);
            Part(transform, "Minaret cap", PrimitiveType.Sphere, new Vector3(x + 13, 16, z + 5), new Vector3(3.5f, 4, 3.5f), gold);
            for (int i = -1; i <= 1; i++)
            {
                Box("Mosque arch doorway", new Vector3(x + i * 6, 1.5f, z - 10.1f), new Vector3(3, 3, .1f), dark);
                Part(transform, "Arch crown", PrimitiveType.Sphere, new Vector3(x + i * 6, 3, z - 10.1f), new Vector3(3, 3, .14f), dark);
            }
            Sign("MOSQUE", new Vector3(x, 1.6f, z - 14.5f), 12, .8f);
        }

        void Athletics()
        {
            Reserve("Games Ground", 32, 125, 138, 91, new Color(.36f, .55f, .28f));
            var track = Material("Running track", new Color(.62f, .26f, .2f));
            Ring("400 m track", new Vector3(32, .07f, 125), 68, 43, 11, track);
            for (int lane = 0; lane <= 5; lane++) Ring("Track lane", new Vector3(32, .08f, 125), 68 - lane * 2, 43 - lane * 2, .13f, white);
            var pitch = Material("Football grass", new Color(.24f, .43f, .19f));
            Box("Football pitch", new Vector3(32, .06f, 125), new Vector3(89, .1f, 48), pitch);
            for (int stripe = 0; stripe < 8; stripe++)
                if (stripe % 2 == 0) Box("Mown grass stripe", new Vector3(-7 + stripe * 11.1f, .118f, 125), new Vector3(11, .012f, 47), grass);
            CourtLines(32, 125, 88, 47);
            Box("Halfway line", new Vector3(32, .14f, 125), new Vector3(.15f, .02f, 47), white);
            Ring("Centre circle", new Vector3(32, .14f, 125), 7, 7, .15f, white);
            for (int side = -1; side <= 1; side += 2)
            {
                float x = 32 + side * 44;
                CourtLines(x - side * 7, 125, 14, 27);
                Beam("Goal post", new Vector3(x, 0, 121), new Vector3(x, 3.2f, 121), .18f, white);
                Beam("Goal post", new Vector3(x, 0, 129), new Vector3(x, 3.2f, 129), .18f, white);
                Beam("Goal crossbar", new Vector3(x, 3.2f, 121), new Vector3(x, 3.2f, 129), .18f, white);
                for (int i = -4; i <= 4; i++) Beam("Goal net", new Vector3(x + side * 2, .15f, 125 + i), new Vector3(x, 3.2f, 125 + i), .03f, white);
            }
            Sign("GAMES GROUND  /  400 M TRACK", new Vector3(32, 2, 78), 34, .65f);
        }

        void Courts()
        {
            Court("Volleyball", 145, 137, 45, 30, new Color(.68f, .48f, .28f));
            Net(145, 137, 23, 2.5f);
            Court("Basketball", 145, 94, 46, 31, new Color(.59f, .29f, .2f));
            for (int side = -1; side <= 1; side += 2)
            {
                float x = 145 + side * 20;
                Beam("Basketball support", new Vector3(x, 0, 94), new Vector3(x, 4.5f, 94), .22f, dark);
                Box("Backboard", new Vector3(x, 4.2f, 94), new Vector3(.15f, 1.8f, 3), white);
                Ring("Basketball rim", new Vector3(x - side * .9f, 3.5f, 94), .65f, .65f, .09f, gold);
            }
            Court("Tennis Court", 143, -54, 57, 35, new Color(.22f, .4f, .38f));
            Net(143, -54, 30, 1.2f);
        }

        void Court(string name, float x, float z, float w, float d, Color color)
        {
            Reserve(name, x, z, w, d, color);
            Box(name + " surface", new Vector3(x, .08f, z), new Vector3(w, .16f, d), Material(name, color));
            CourtLines(x, z, w - 5, d - 5);
            Box("Court centre", new Vector3(x, .18f, z), new Vector3(.15f, .02f, d - 5), white);
            Fence(x, z, w + 1, d + 1);
            Sign(name.ToUpperInvariant(), new Vector3(x, 1.8f, z - d / 2 - 2), 24, .7f);
        }

        void CourtLines(float x, float z, float w, float d)
        {
            Box("Court line", new Vector3(x, .18f, z - d / 2), new Vector3(w, .02f, .15f), white);
            Box("Court line", new Vector3(x, .18f, z + d / 2), new Vector3(w, .02f, .15f), white);
            Box("Court line", new Vector3(x - w / 2, .18f, z), new Vector3(.15f, .02f, d), white);
            Box("Court line", new Vector3(x + w / 2, .18f, z), new Vector3(.15f, .02f, d), white);
        }

        void Net(float x, float z, float length, float height)
        {
            for (int side = -1; side <= 1; side += 2)
                Beam("Net post", new Vector3(x, 0, z + side * length / 2), new Vector3(x, height + .4f, z + side * length / 2), .12f, white);
            for (int i = 0; i < 4; i++) Beam("Net cord", new Vector3(x, height - i * .22f, z - length / 2), new Vector3(x, height - i * .22f, z + length / 2), .035f, dark);
            for (float i = -length / 2; i < length / 2; i += .6f)
                Beam("Net mesh", new Vector3(x, height - .7f, z + i), new Vector3(x, height, z + i), .025f, dark);
        }

        void Fence(float x, float z, float width, float depth)
        {
            // Open southern edge lets the player walk into the courts.
            for (float f = -width / 2; f <= width / 2; f += 5)
                Beam("Fence post", new Vector3(x + f, 0, z + depth / 2), new Vector3(x + f, 3, z + depth / 2), .1f, dark);
            for (int side = -1; side <= 1; side += 2)
            {
                for (float f = -depth / 2; f <= depth / 2; f += 5)
                    Beam("Fence post", new Vector3(x + side * width / 2, 0, z + f), new Vector3(x + side * width / 2, 3, z + f), .1f, dark);
                for (int i = 1; i <= 3; i++)
                    Beam("Fence rail", new Vector3(x + side * width / 2, i, z - depth / 2), new Vector3(x + side * width / 2, i, z + depth / 2), .045f, dark);
            }
            for (int i = 1; i <= 3; i++) Beam("Fence rail", new Vector3(x - width / 2, i, z + depth / 2), new Vector3(x + width / 2, i, z + depth / 2), .045f, dark);
        }

        void Pool(float x, float z)
        {
            Reserve("Swimming Pool", x, z, 48, 28, new Color(.17f, .63f, .77f));
            Box("Pool terrace", new Vector3(x, .12f, z), new Vector3(56, .24f, 35), trim);
            var water = Material("Pool water", new Color(.08f, .52f, .68f), .88f);
            Box("Pool surface", new Vector3(x, .26f, z), new Vector3(44, .03f, 25), water);
            for (int i = -3; i <= 3; i++)
            {
                Box("Pool lane", new Vector3(x, .28f, z + i * 3), new Vector3(44, .02f, .09f), white);
                Box("Pool end marker", new Vector3(x + 19, .29f, z + i * 3), new Vector3(.12f, .02f, 1.8f), dark);
            }
            for (int side = -1; side <= 1; side += 2)
                Box("Pool coping", new Vector3(x + side * 22.6f, .32f, z), new Vector3(1.1f, .35f, 26), white, true);
            Sign("SWIMMING POOL", new Vector3(x, 1.8f, z - 19), 24, .75f);
        }

        void DrillGround()
        {
            Reserve("Drill Ground", -128, 86, 95, 31, new Color(.47f, .48f, .43f));
            Box("Parade square", new Vector3(-128, .05f, 86), new Vector3(95, .1f, 31), asphalt);
            for (int x = -167; x <= -87; x += 8)
                for (int z = 76; z <= 96; z += 4)
                    Box("Parade markings", new Vector3(x, .12f, z), new Vector3(2, .015f, .13f), white);
            Sign("DRILL GROUND", new Vector3(-128, 1.6f, 68), 25, .8f);
        }

        void ObstacleGround()
        {
            Reserve("Obstacle Ground", -128, 120, 95, 27, new Color(.64f, .49f, .29f));
            Box("Obstacle sand", new Vector3(-128, .05f, 120), new Vector3(95, .1f, 27), Material("Sand", new Color(.59f, .49f, .31f)));
            for (int i = 0; i < 6; i++)
            {
                float x = -164 + i * 14;
                float height = 1.5f + (i % 3) * .7f;
                Box("Training hurdle", new Vector3(x, height / 2, 120), new Vector3(.6f, height, 6), bark, true);
                for (int j = 0; j < 3; j++)
                    Box("Stepping block", new Vector3(x, .3f, 128 - j * 2), new Vector3(1.1f, .6f, 1.1f), cream, true);
            }
            Reserve("Mini ISSB Ground", -128, 155, 95, 27, new Color(.45f, .57f, .28f));
            for (int i = 0; i < 4; i++)
            {
                float x = -163 + i * 22;
                Beam("Training frame", new Vector3(x - 3, 0, 155), new Vector3(x - 3, 4, 155), .25f, bark);
                Beam("Training frame", new Vector3(x + 3, 0, 155), new Vector3(x + 3, 4, 155), .25f, bark);
                Beam("Training frame", new Vector3(x - 3, 4, 155), new Vector3(x + 3, 4, 155), .25f, bark);
                for (int rope = -2; rope <= 2; rope++) Beam("Climbing ropes", new Vector3(x + rope, .5f, 155), new Vector3(x + rope, 4, 155), .06f, trim);
            }
            Sign("OBSTACLE GROUND", new Vector3(-128, 1.8f, 104), 28, .7f);
            Sign("MINI ISSB GROUND", new Vector3(-128, 1.8f, 139), 28, .7f);
        }

        void MainGate()
        {
            Reserve("Main Gate", 0, -158, 30, 10, new Color(.92f, .67f, .26f));
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Gatehouse", new Vector3(side * 14, 3, -159), new Vector3(9, 6, 9), cream, true);
                Roof(side * 14, 6.1f, -159, 11, 11, 2.6f);
                Box("Gatehouse window", new Vector3(side * 14, 3, -163.6f), new Vector3(2.5f, 2.5f, .1f), glass);
            }
            Box("Welcome arch", new Vector3(0, 8, -158), new Vector3(22, 1.6f, 1.5f), dark, true);
            Sign("CADET COLLEGE", new Vector3(0, 8, -159), 22, .95f);
            // Bangladesh flag beside the approach, away from the driving lane.
            Part(transform, "Flagpole", PrimitiveType.Cylinder, new Vector3(19, 7, -129), new Vector3(.16f, 7, .16f), trim, true);
            Box("Bangladesh flag", new Vector3(21.5f, 12.5f, -129), new Vector3(5, 3, .06f), Material("Flag green", new Color(0, .4f, .28f)));
            var disc = Part(transform, "Flag red disc", PrimitiveType.Cylinder, new Vector3(21.2f, 12.5f, -129.05f), new Vector3(1.7f, .025f, 1.7f), Material("Flag red", new Color(.86f, .1f, .17f)));
            disc.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }

        void Boundary()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                Box("Boundary wall", new Vector3(side * 195, 1.6f, 8), new Vector3(1, 3.2f, 347), cream, true);
                Box("Boundary coping", new Vector3(side * 195, 3.3f, 8), new Vector3(1.5f, .25f, 347), trim);
                Box("South boundary", new Vector3(side * 108, 1.6f, -165), new Vector3(175, 3.2f, 1), cream, true);
                for (int z = -165; z <= 180; z += 15)
                    Box("Wall pillar", new Vector3(side * 195, 2, z), new Vector3(1.8f, 4, 1.8f), trim, true);
            }
            Box("North boundary", new Vector3(0, 1.6f, 181), new Vector3(390, 3.2f, 1), cream, true);
            // Close the short arrival apron; the playable world is the campus.
            Box("Arrival apron", new Vector3(0, .02f, -176), new Vector3(40, .04f, 22), asphalt);
            Box("Outer boundary", new Vector3(0, 1, -187), new Vector3(400, 2, 1), cream, true);
        }

        void Gardens()
        {
            Random.State saved = Random.state;
            Random.InitState(121);
            for (int i = 0; i < 350; i++)
            {
                Vector2 point = new Vector2(Random.Range(-188f, 188), Random.Range(-154f, 175));
                bool blocked = false;
                foreach (var area in reserved) if (area.Contains(point)) { blocked = true; break; }
                if (!blocked) Tree(point.x, point.y, Random.Range(.7f, 1.4f), false);
            }
            for (int i = 0; i < 10; i++)
                for (int side = -1; side <= 1; side += 2)
                    Tree(side * 20, -115 + i * 7, 1, true);
            for (int i = -220; i < 240; i += 12)
            {
                Tree(i, 197 + Random.Range(0, 20), Random.Range(1f, 1.7f), false);
                Tree(i, -204 - Random.Range(0, 20), Random.Range(1f, 1.7f), false);
            }
            for (int i = -170; i < 185; i += 12)
            {
                Tree(-212 - Random.Range(0, 15), i, Random.Range(1f, 1.7f), false);
                Tree(212 + Random.Range(0, 15), i, Random.Range(1f, 1.7f), false);
            }
            Random.state = saved;
            // A central garden between the houses.
            Ring("House garden", new Vector3(-111, .06f, -22), 9, 9, 2, pavement);
            Part(transform, "Garden planter", PrimitiveType.Cylinder, new Vector3(-111, .3f, -22), new Vector3(4, .3f, 4), trim, true);
            Tree(-111, -22, .65f, false);
        }

        void Tree(float x, float z, float scale, bool palm)
        {
            float height = (palm ? 9 : 5.5f) * scale;
            Part(transform, "Tree trunk", PrimitiveType.Cylinder, new Vector3(x, height / 2, z), new Vector3(.65f * scale, height / 2, .65f * scale), bark, true);
            if (palm)
            {
                for (int leaf = 0; leaf < 7; leaf++)
                {
                    float angle = leaf * Mathf.PI * 2 / 7;
                    Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    var frond = Part(transform, "Palm frond", PrimitiveType.Sphere, new Vector3(x, height, z) + dir * 1.7f, new Vector3(1.2f, .55f, 6), foliage);
                    frond.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(12, 0, 0);
                }
            }
            else
            {
                var leafColor = new Color(.22f + scale * .06f, .32f + scale * .06f, .1f + scale * .025f);
                var leafMaterial = Material("Canopy " + Mathf.RoundToInt(scale * 10), leafColor);
                Part(transform, "Canopy", PrimitiveType.Sphere, new Vector3(x, height, z), new Vector3(6 * scale, 5 * scale, 5 * scale), leafMaterial);
                Part(transform, "Canopy crown", PrimitiveType.Sphere, new Vector3(x + 1.5f * scale, height + scale, z - scale), new Vector3(4 * scale, 4 * scale, 4 * scale), leafMaterial);
            }
        }

        void StreetFurniture()
        {
            for (int i = 0; i < 13; i++)
            {
                float z = -116 + i * 14;
                for (int side = -1; side <= 1; side += 2)
                {
                    float x = side < 0 ? -63 : 107;
                    Part(transform, "Lamp post", PrimitiveType.Cylinder, new Vector3(x, 3.4f, z), new Vector3(.14f, 3.4f, .14f), dark);
                    Box("Lamp head", new Vector3(x, 6.8f, z), new Vector3(1, .3f, 1), trim);
                }
            }
            for (int i = 0; i < 6; i++)
            {
                float x = -32 + i * 17;
                Box("Bench seat", new Vector3(x, .65f, 18), new Vector3(3, .18f, .85f), bark, true);
                Box("Bench back", new Vector3(x, 1.15f, 18.35f), new Vector3(3, .8f, .12f), bark);
                for (int side = -1; side <= 1; side += 2)
                    Box("Bench leg", new Vector3(x + side, .3f, 18), new Vector3(.15f, .6f, .7f), dark);
            }
        }

        void Beam(string name, Vector3 a, Vector3 b, float thickness, UnityEngine.Material material)
        {
            var beam = Part(transform, name, PrimitiveType.Cylinder, (a + b) * .5f, new Vector3(thickness, Vector3.Distance(a, b) * .5f, thickness), material);
            beam.transform.rotation = Quaternion.FromToRotation(Vector3.up, b - a);
        }

        void Ring(string name, Vector3 center, float xRadius, float zRadius, float width, UnityEngine.Material material)
        {
            const int segments = 96;
            var vertices = new Vector3[(segments + 1) * 2];
            var triangles = new int[segments * 6];
            for (int i = 0; i <= segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                vertices[i * 2] = new Vector3(Mathf.Cos(angle) * xRadius, 0, Mathf.Sin(angle) * zRadius);
                vertices[i * 2 + 1] = new Vector3(Mathf.Cos(angle) * (xRadius - width), 0, Mathf.Sin(angle) * (zRadius - width));
                if (i == segments) continue;
                int v = i * 2, t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals();
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = center;
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            go.AddComponent<MeshRenderer>().sharedMaterial = material;
        }

        public Transform CreateObjectiveMarker()
        {
            var marker = new GameObject("Gold mission beacon").transform;
            marker.SetParent(transform, false);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 90;
                Vector3 p = Quaternion.Euler(0, angle, 0) * new Vector3(0, 2.1f, 2.3f);
                Part(marker, "Beacon pillar", PrimitiveType.Cube, p, new Vector3(.22f, 4, .22f), gold);
            }
            var diamond = Part(marker, "Mission diamond", PrimitiveType.Cube, Vector3.up * 5.2f, Vector3.one * .8f, gold);
            diamond.transform.localRotation = Quaternion.Euler(45, 0, 45);
            return marker;
        }
    }
}
