// =============================================================
// GameBootstrap.cs — builds the entire test arena at runtime: camera,
// ground, two local co-op players, and the three test enemies. Uses
// [RuntimeInitializeOnLoadMethod] so it auto-runs on Play in ANY scene
// (even an empty one) — no manual scene/prefab wiring required.
//
// Tweak PLAYER_CHARS to choose which roles the two players control.
// Press R to reset, [ and ] to cycle Player 1's character.
// =============================================================
using UnityEngine;
using System.Collections.Generic;

namespace LF2
{
    public class GameBootstrap : MonoBehaviour
    {
        // which roles the two players start as (any id from Roster)
        static readonly string[] PLAYER_CHARS = { "ninja", "tank" };

        Arena _arena;
        readonly List<CharacterDef> _chars = Roster.Characters();
        int _p1Index = 0;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (FindFirstObjectByType<GameBootstrap>() != null) return;
            var go = new GameObject("LF2_Bootstrap");
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            Time.fixedDeltaTime = 1f / 60f;           // match JS per-frame units
            _p1Index = Mathf.Max(0, _chars.FindIndex(c => c.Id == PLAYER_CHARS[0]));
            EnsureCamera();
            BuildArena();
            SpawnAll();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.R)) { Reset(); }
            if (Input.GetKeyDown(KeyCode.LeftBracket))  { _p1Index = (_p1Index - 1 + _chars.Count) % _chars.Count; Reset(); }
            if (Input.GetKeyDown(KeyCode.RightBracket)) { _p1Index = (_p1Index + 1) % _chars.Count; Reset(); }
        }

        void Reset()
        {
            if (_arena) Destroy(_arena.gameObject);
            BuildArena();
            SpawnAll();
        }

        void EnsureCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 360;               // ~720px tall view
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

            // floor strip
            var floor = Util.Box(go.transform, 1200, 24, new Color(0.16f, 0.18f, 0.24f), order: 0);
            floor.transform.position = new Vector3(0, _arena.GroundY - 12, 0);

            // HUD
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

            // real art + procedural animation, if this character has a sprite
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
            // ----- Player 1: WASD + F/G/H ; Player 2: arrows + ./,/M -----
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

            // ----- enemies -----
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
