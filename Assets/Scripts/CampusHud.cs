using UnityEngine;

namespace CadetCampus
{
    public sealed class CampusHud : MonoBehaviour
    {
        CampusGame game;
        GUIStyle text, small, title, heading, button;
        readonly Color ink = new Color(.035f, .075f, .085f, .94f);
        readonly Color muted = new Color(.64f, .73f, .7f);
        readonly Color accent = new Color(1, .76f, .27f);
        readonly Color mint = new Color(.36f, .84f, .73f);
        float width, height;

        void Awake() { game = GetComponent<CampusGame>(); }

        void Styles()
        {
            if (text != null) return;
            text = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true, richText = false };
            text.normal.textColor = Color.white;
            small = new GUIStyle(text) { fontSize = 12 };
            title = new GUIStyle(text) { fontSize = 58, fontStyle = FontStyle.Bold, wordWrap = false };
            heading = new GUIStyle(text) { fontSize = 23, fontStyle = FontStyle.Bold };
            button = new GUIStyle(GUI.skin.button) { fontSize = 16, fontStyle = FontStyle.Bold, fixedHeight = 48 };
            button.normal.textColor = Color.white;
            button.hover.textColor = accent;
        }

        void OnGUI()
        {
            if (!game || game.Player == null) return;
            Styles();
            float scale = Mathf.Min(Screen.width / 1280f, Screen.height / 800f);
            if (scale <= 0) return;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            width = Screen.width / scale;
            height = Screen.height / scale;
            if (!game.Started) { Welcome(); return; }
            Header();
            ObjectivePanel();
            DrawMap(new Rect(width - 246, height - 242, 214, 210), false);
            Label(new Rect(width - 242, height - 27, 214, 22), "M  CAMPUS MAP     N ↑", small, muted);
            if (game.ActiveCar) Speedometer();
            if (game.AcceptInput)
            {
                string prompt = null;
                if (game.NearObjective && !game.Rules.CircuitRunning)
                    prompt = game.Rules.Stage == MissionStage.FindCar || game.Rules.Stage == MissionStage.Complete
                        ? (game.ActiveCar ? "E   START CAMPUS CIRCUIT" : "FIND A CAR TO START THE CIRCUIT")
                        : game.ActiveCar && game.Rules.Stage != MissionStage.Orientation ? "F   EXIT TO INTERACT" : "E   INTERACT";
                else if (!game.ActiveCar && game.NearestCar) prompt = "F   ENTER " + game.NearestCar.name.ToUpperInvariant();
                if (prompt != null)
                {
                    Panel(new Rect(width / 2 - 220, height - 92, 440, 43), ink);
                    Center(new Rect(width / 2 - 215, height - 82, 430, 30), prompt, text, accent);
                }
                if (Time.unscaledTime < game.ToastUntil)
                {
                    Panel(new Rect(width / 2 - 310, 94, 620, 54), ink);
                    Center(new Rect(width / 2 - 295, 107, 590, 35), game.Toast, text, Color.white);
                }
            }
            if (game.MapOpen) MapOverlay();
            if (game.Paused) Pause();
        }

        void Welcome()
        {
            Panel(new Rect(0, 0, width, height), new Color(.02f, .055f, .06f, .4f));
            Panel(new Rect(0, 0, 610, height), new Color(.025f, .055f, .065f, .94f));
            Label(new Rect(52, 50, 510, 24), "A CAMPUS. A SET OF KEYS. YOUR AFTERNOON.", small, mint);
            Label(new Rect(47, 129, 550, 75), "CADET", title, Color.white);
            Label(new Rect(50, 204, 550, 75), "AFTER HOURS", title, accent);
            Panel(new Rect(54, 308, 62, 4), mint);
            Label(new Rect(53, 337, 490, 70), "Explore a leafy college campus on foot or behind the wheel. Run a delivery. Beat the campus circuit. Find your own route.", text, new Color(.8f, .84f, .79f));
            if (GUI.Button(new Rect(54, 445, 300, 48), "ENTER CAMPUS  →", button)) game.Begin();
            Label(new Rect(54, 526, 490, 85), "WASD  Move / drive      MOUSE  Look\nSHIFT  Sprint     SPACE  Jump / handbrake\nF  Enter / exit car     E  Interact     M  Map\nR  Recover     ESC  Pause", text, muted);
            Label(new Rect(54, height - 65, 480, 40), "UNITY SANDBOX PROTOTYPE  /  ORIGINAL PROCEDURAL ART\nInspired by the supplied Cadet College campus map.", small, muted);
            Label(new Rect(width - 262, 52, 230, 26), "FREE ROAM  /  SINGLE PLAYER", small, Color.white);
        }

        void Header()
        {
            Panel(new Rect(24, 22, 318, 64), ink);
            Panel(new Rect(24, 22, 4, 64), accent);
            Label(new Rect(42, 29, 290, 30), "CADET / AFTER HOURS", heading, Color.white);
            Label(new Rect(43, 62, 280, 20), "CAMPUS SANDBOX     •     FREE ROAM", small, mint);
            Panel(new Rect(width - 227, 22, 203, 64), ink);
            Label(new Rect(width - 211, 31, 175, 28), game.Rules.Credits.ToString("N0") + "  CREDITS", heading, accent);
            Label(new Rect(width - 209, 62, 180, 20), game.ActiveCar ? "ON THE ROAD" : "ON FOOT  /  CADET", small, muted);
        }

        void ObjectivePanel()
        {
            Rect box = new Rect(24, height - 228, 340, 196);
            Panel(box, ink);
            Panel(new Rect(box.x, box.y, box.width, 3), accent);
            Label(new Rect(box.x + 16, box.y + 16, 310, 22), "CURRENT OBJECTIVE", small, mint);
            Label(new Rect(box.x + 16, box.y + 43, 310, 32), game.ObjectiveTitle, heading, Color.white);
            Label(new Rect(box.x + 16, box.y + 82, 307, 63), game.ObjectiveDetail, text, new Color(.79f, .84f, .8f));
            int distance = Mathf.RoundToInt(Vector3.Distance(game.Position, game.Objective));
            string status = game.Rules.CircuitRunning ? Mathf.CeilToInt(game.Rules.Remaining) + " s REMAINING" : distance + " m TO MARKER";
            Label(new Rect(box.x + 16, box.y + 160, 307, 24), status, text, accent);
            Label(new Rect(28, height - 261, 450, 22), "F  CAR     E  INTERACT     R  RECOVER     ESC  PAUSE", small, Color.white);
        }

        void Speedometer()
        {
            Panel(new Rect(width - 246, height - 337, 214, 81), ink);
            Label(new Rect(width - 230, height - 332, 135, 56), Mathf.RoundToInt(game.ActiveCar.SpeedKph).ToString("00"), title, Color.white);
            Label(new Rect(width - 113, height - 308, 75, 30), "KM/H", small, mint);
        }

        void Pause()
        {
            Panel(new Rect(0, 0, width, height), new Color(.015f, .035f, .045f, .88f));
            float x = width / 2 - 220;
            Label(new Rect(x, height / 2 - 185, 500, 70), "TAKE A BREAK", title, Color.white);
            Label(new Rect(x, height / 2 - 98, 450, 62), "WASD  Move / drive     MOUSE  Look\nF  Car     E  Interact     M  Map     R  Recover", text, muted);
            if (GUI.Button(new Rect(x, height / 2, 440, 48), "RESUME", button)) game.Resume();
            if (GUI.Button(new Rect(x, height / 2 + 62, 440, 48), game.Muted ? "SOUND: OFF" : "SOUND: ON", button)) game.ToggleMute();
            Label(new Rect(x, height / 2 + 136, 440, 50), "Progress lasts for this session. ESC also resumes.", small, muted);
        }

        void MapOverlay()
        {
            Panel(new Rect(0, 0, width, height), new Color(.02f, .045f, .05f, .98f));
            Label(new Rect(32, 24, 800, 50), "YOUR CAMPUS. YOUR ROUTE.", heading, Color.white);
            Label(new Rect(34, 61, 750, 28), "NORTH ↑      GOLD = OBJECTIVE      MINT = YOU      WHITE = PARKED CAR", small, mint);
            float mapSize = Mathf.Min(height - 148, width - 375);
            DrawMap(new Rect(34, 109, mapSize, mapSize), true);
            float x = mapSize + 62;
            Label(new Rect(x, 113, 285, 32), "CAMPUS DIRECTORY", heading, accent);
            float y = 163;
            foreach (var landmark in game.World.Landmarks)
            {
                Panel(new Rect(x, y + 5, 8, 8), landmark.Color);
                Label(new Rect(x + 20, y, 255, 25), landmark.Name, text, Color.white);
                y += 27;
            }
            if (GUI.Button(new Rect(width - 253, 25, 220, 48), "CLOSE MAP  [M]", button)) game.Resume();
        }

        Vector2 MapPoint(Rect area, Vector2 world)
        {
            return new Vector2(area.x + (world.x + 200) / 400 * area.width, area.y + (185 - world.y) / 370 * area.height);
        }

        void DrawMap(Rect outer, bool full)
        {
            Panel(outer, new Color(.13f, .22f, .17f, .98f));
            Rect area = new Rect(outer.x + 8, outer.y + 8, outer.width - 16, outer.height - 16);
            Panel(new Rect(area.x, area.y, area.width, 1), new Color(.52f, .61f, .44f));
            foreach (var road in game.World.Roads)
            {
                Vector2 p = MapPoint(area, new Vector2(road.xMin, road.yMax));
                Panel(new Rect(p.x, p.y, road.width / 400 * area.width, road.height / 370 * area.height), new Color(.47f, .5f, .44f));
            }
            foreach (var landmark in game.World.Landmarks)
            {
                Vector2 p = MapPoint(area, landmark.Position + new Vector2(-landmark.Size.x, landmark.Size.y) * .5f);
                var r = new Rect(p.x, p.y, landmark.Size.x / 400 * area.width, landmark.Size.y / 370 * area.height);
                Panel(r, landmark.Color);
                if (full && r.width > 38)
                {
                    GUIStyle name = new GUIStyle(small) { fontSize = 10, alignment = TextAnchor.MiddleCenter };
                    Label(r, landmark.Name, name, Color.white);
                }
            }
            foreach (var car in game.Cars)
            {
                if (car == game.ActiveCar) continue;
                Vector2 p = MapPoint(area, new Vector2(car.transform.position.x, car.transform.position.z));
                Panel(new Rect(p.x - 2, p.y - 3, 4, 6), Color.white);
            }
            Vector2 goal = MapPoint(area, new Vector2(game.Objective.x, game.Objective.z));
            float pulse = 6 + Mathf.Sin(Time.unscaledTime * 3);
            Panel(new Rect(goal.x - pulse, goal.y - pulse, pulse * 2, pulse * 2), new Color(1, .76f, .27f, .25f));
            Panel(new Rect(goal.x - 3, goal.y - 3, 6, 6), accent);
            Vector2 player = MapPoint(area, new Vector2(game.Position.x, game.Position.z));
            Matrix4x4 original = GUI.matrix;
            GUIUtility.RotateAroundPivot(game.Heading, player);
            Center(new Rect(player.x - 10, player.y - 12, 20, 24), "▲", text, mint);
            GUI.matrix = original;
        }

        static void Panel(Rect area, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        static void Label(Rect area, string value, GUIStyle style, Color color)
        {
            Color previous = style.normal.textColor;
            style.normal.textColor = color;
            GUI.Label(area, value, style);
            style.normal.textColor = previous;
        }

        static void Center(Rect area, string value, GUIStyle style, Color color)
        {
            TextAnchor previous = style.alignment;
            style.alignment = TextAnchor.MiddleCenter;
            Label(area, value, style, color);
            style.alignment = previous;
        }
    }
}
