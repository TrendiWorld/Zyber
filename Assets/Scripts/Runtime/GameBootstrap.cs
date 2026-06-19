// =============================================================
// GameBootstrap.cs — builds the test arena at runtime and runs the simple
// game-state machine + menus (Title / Playing / Paused / Cleared). Uses
// [RuntimeInitializeOnLoadMethod] so it auto-runs on Play in ANY scene
// (even an empty one) — no manual scene/prefab wiring required.
//
// Tweak PLAYER_CHARS to choose which roles the two players control.
// In-game: R restart · [ and ] cycle Player 1's character · Esc pause.
// =============================================================
using UnityEngine;
using System.Collections.Generic;

namespace LF2
{
    public class GameBootstrap : MonoBehaviour
    {
        enum Phase { Title, Playing, Paused, Cleared }

        // which roles the two players start as (any id from Roster)
        static readonly string[] PLAYER_CHARS = { "ninja", "tank" };

        Arena _arena;
        readonly List<CharacterDef> _chars = Roster.Characters();
        int _p1Index = 0;
        Phase _phase = Phase.Title;

        Texture2D _dim;
        GUIStyle _title, _btn, _hint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (FindFirstObjectByType<GameBootstrap>() != null) return;
            new GameObject("LF2_Bootstrap").AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            Time.fixedDeltaTime = 1f / 60f;
            _p1Index = Mathf.Max(0, _chars.FindIndex(c => c.Id == PLAYER_CHARS[0]));
            EnsureCamera();
            _dim = new Texture2D(1, 1); _dim.SetPixel(0, 0, Color.white); _dim.Apply();
        }

        void Update()
        {
            if (_phase == Phase.Playing)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) { SetPhase(Phase.Paused); return; }
                if (Input.GetKeyDown(KeyCode.R)) { StartMatch(); return; }
                if (Input.GetKeyDown(KeyCode.LeftBracket))  { _p1Index = (_p1Index - 1 + _chars.Count) % _chars.Count; StartMatch(); return; }
                if (Input.GetKeyDown(KeyCode.RightBracket)) { _p1Index = (_p1Index + 1) % _chars.Count; StartMatch(); return; }

                // win when every enemy is down
                if (_arena != null && _arena.Hostiles(Team.Players).Count == 0)
                    SetPhase(Phase.Cleared);
            }
            else if (_phase == Phase.Paused && Input.GetKeyDown(KeyCode.Escape))
            {
                SetPhase(Phase.Playing);
            }
        }

        void SetPhase(Phase p)
        {
            _phase = p;
            Time.timeScale = (p == Phase.Playing) ? 1f : 0f;
        }

        void StartMatch()
        {
            if (_arena) Destroy(_arena.gameObject);
            BuildArena();
            SpawnAll();
            SetPhase(Phase.Playing);
        }

        // ---------------------------------------------------------- MENUS
        void OnGUI()
        {
            _title ??= new GUIStyle(GUI.skin.label) { fontSize = 64, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(1f, 0.3f, 0.3f) } };
            _btn   ??= new GUIStyle(GUI.skin.button) { fontSize = 22, fontStyle = FontStyle.Bold };
            _hint  ??= new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.8f, 0.8f, 0.85f) } };

            if (_phase == Phase.Playing)
            {
                if (GUI.Button(new Rect(Screen.width - 96, 10, 86, 26), "Menu")) SetPhase(Phase.Paused);
                return;
            }

            // dim backdrop for any overlay screen
            GUI.color = new Color(0f, 0f, 0f, _phase == Phase.Title ? 0.92f : 0.7f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), _dim);
            GUI.color = Color.white;

            float cx = Screen.width / 2f, cy = Screen.height / 2f;
            GUI.Label(new Rect(cx - 300, cy - 150, 600, 90), "ZYBER", _title);

            string sub = _phase == Phase.Cleared ? "Enemies cleared!"
                       : _phase == Phase.Paused  ? "Paused"
                       : "A co-op assassin brawler";
            GUI.Label(new Rect(cx - 300, cy - 64, 600, 24), sub, _hint);

            string primary = _phase == Phase.Title ? "PLAY" : "RESTART";
            if (GUI.Button(new Rect(cx - 110, cy - 20, 220, 52), primary)) StartMatch();

            if (_phase == Phase.Paused &&
                GUI.Button(new Rect(cx - 110, cy + 42, 220, 44), "Resume")) SetPhase(Phase.Playing);

            GUI.Label(new Rect(cx - 300, cy + 110, 600, 22),
                "P1: A/D move · W jump · F slash · G shuriken · H blade rush", _hint);
            GUI.Label(new Rect(cx - 300, cy + 134, 600, 22),
                "In-game: Esc pause · R restart · [ ] swap P1 character", _hint);
        }

        // ---------------------------------------------------------- BUILD
        void EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera") { tag = "MainCamera" };
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 360;
            cam.backgroundColor = new Color(0.07f, 0.08f, 0.12f);
            cam.transform.position = new Vector3(0, 0, -10);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        void BuildArena()
        {
            var go = new GameObject("Arena");
            _arena = go.AddComponent<Arena>();
            _arena.GroundY = -180f;
            _arena.MinX = -560f; _arena.MaxX = 560f;

            var floor = Util.Box(go.transform, 1200, 24, new Color(0.16f, 0.18f, 0.24f), order: 0);
            floor.transform.position = new Vector3(0, _arena.GroundY - 12, 0);

            go.AddComponent<Hud>().Arena = _arena;
        }

        Fighter SpawnFighter(Team team, CharacterDef c, float x)
        {
            var go = new GameObject("Player_" + c.Id);
            go.transform.SetParent(_arena.transform, false);
            var f = go.AddComponent<Fighter>();
            f.Init(_arena, team, c.Name, Util.Hex(c.Color),
                   c.HurtW, c.HurtH, c.MaxHp, c.MaxEnergy, c.EnergyRegen, c.Speed, c.Jump, c.Weight);
            f.PosX = x; f.Facing = team == Team.Players ? 1 : -1;

            if (!string.IsNullOrEmpty(c.SpriteFile))
            {
                f.AttachSprite(c.SpriteFile, c.SpriteScale, c.SpriteYOffset, c.SpriteNativeFacing);
                f.gameObject.AddComponent<ProcAnimator>();
            }

            _arena.Add(f);
            return f;
        }

        void SpawnAll()
        {
            var c1 = _chars[_p1Index];
            var c2 = Roster.GetCharacter(PLAYER_CHARS[1]) ?? _chars[1 % _chars.Count];

            var f1 = SpawnFighter(Team.Players, c1, -200);
            f1.gameObject.AddComponent<PlayerController>().Setup(f1, c1, new KeyMap {
                Left = KeyCode.A, Right = KeyCode.D, Jump = KeyCode.W,
                Attack = KeyCode.F, Ab1 = KeyCode.G, Ab2 = KeyCode.H });

            var f2 = SpawnFighter(Team.Players, c2, -80);
            f2.gameObject.AddComponent<PlayerController>().Setup(f2, c2, new KeyMap {
                Left = KeyCode.LeftArrow, Right = KeyCode.RightArrow, Jump = KeyCode.UpArrow,
                Attack = KeyCode.Period, Ab1 = KeyCode.Comma, Ab2 = KeyCode.M });

            float ex = 120;
            foreach (var e in Roster.Enemies())
            {
                var go = new GameObject("Enemy_" + e.Id);
                go.transform.SetParent(_arena.transform, false);
                var f = go.AddComponent<Fighter>();
                f.Init(_arena, Team.Enemies, e.Name, Util.Hex(e.Color),
                       e.W, e.H, e.Hp, 100, 0, 3.0f, 14, e.Weight);
                f.PosX = ex; f.Facing = -1;
                _arena.Add(f);
                go.AddComponent<EnemyController>().Setup(f, e);
                ex += 150;
            }
        }
    }
}
