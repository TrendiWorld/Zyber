// =============================================================
// Fx.cs — throwaway visual effects (aoe ring, laser beam). Pure
// cosmetics that fade out; no gameplay. Box-art placeholders.
// =============================================================
using UnityEngine;

namespace LF2
{
    public class Fx : MonoBehaviour
    {
        float _life, _max;
        SpriteRenderer _sr;

        public static void Ring(Arena arena, float x, float y, float radius, Color color)
        {
            var go = new GameObject("Fx_Ring");
            go.transform.SetParent(arena.transform, false);
            go.transform.position = new Vector3(x, arena.GroundY + y, 0);
            var sr = Util.Box(go.transform, radius * 2, 16, color, order: 3);
            go.AddComponent<Fx>().Begin(sr, 18);
        }

        public static void Beam(Arena arena, float x, float y, float length, Color color)
        {
            var go = new GameObject("Fx_Beam");
            go.transform.SetParent(arena.transform, false);
            go.transform.position = new Vector3(x + length * 0.5f, arena.GroundY + y, 0);
            var sr = Util.Box(go.transform, Mathf.Abs(length), 26, color, order: 9);
            go.AddComponent<Fx>().Begin(sr, 10);
        }

        void Begin(SpriteRenderer sr, float life) { _sr = sr; _life = _max = life; }

        void Update()
        {
            _life -= 1;
            if (_sr) { var c = _sr.color; c.a = Mathf.Clamp01(_life / _max) * 0.6f; _sr.color = c; }
            if (_life <= 0) Destroy(gameObject);
        }
    }
}
