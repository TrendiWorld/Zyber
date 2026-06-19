// =============================================================
// Fx.cs — throwaway visual effects (aoe ring, laser beam, melee slash).
// Pure cosmetics that fade/spin out; no gameplay. Box-art placeholders.
// =============================================================
using UnityEngine;

namespace LF2
{
    public class Fx : MonoBehaviour
    {
        float _life, _max;
        float _spin;          // degrees/sec
        SpriteRenderer _sr;

        public static void Ring(Arena arena, float x, float y, float radius, Color color)
        {
            var go = Make(arena, x, y);
            var sr = Util.Box(go.transform, radius * 2, 16, color, order: 3);
            go.AddComponent<Fx>().Begin(sr, 18, 0);
        }

        public static void Beam(Arena arena, float x, float y, float length, Color color)
        {
            var go = Make(arena, x + length * 0.5f, y);
            var sr = Util.Box(go.transform, Mathf.Abs(length), 26, color, order: 9);
            go.AddComponent<Fx>().Begin(sr, 10, 0);
        }

        // A bright crescent that sweeps across the front of a melee swing.
        public static void Slash(Arena arena, float x, float y, int dir, float range, Color tint)
        {
            var go = Make(arena, x, y);
            // a long thin bright bar, started angled and spun through the arc
            Color c = (tint == Color.white) ? new Color(0.8f, 0.95f, 1f) : Color.Lerp(tint, Color.white, 0.5f);
            var sr = Util.Box(go.transform, Mathf.Max(70, range), 10, c, order: 9);
            go.transform.localRotation = Quaternion.Euler(0, 0, dir * 55f);
            go.AddComponent<Fx>().Begin(sr, 9, -dir * 900f); // spin down through the swing
        }

        static GameObject Make(Arena arena, float x, float y)
        {
            var go = new GameObject("Fx");
            go.transform.SetParent(arena.transform, false);
            go.transform.position = new Vector3(x, arena.GroundY + y, 0);
            return go;
        }

        void Begin(SpriteRenderer sr, float life, float spin)
        {
            _sr = sr; _life = _max = life; _spin = spin;
        }

        void Update()
        {
            _life -= 1;
            if (_spin != 0) transform.Rotate(0, 0, _spin * Time.deltaTime);
            if (_sr) { var c = _sr.color; c.a = Mathf.Clamp01(_life / _max) * 0.7f; _sr.color = c; }
            if (_life <= 0) Destroy(gameObject);
        }
    }
}
