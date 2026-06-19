// =============================================================
// Arena.cs — the world: ground line, horizontal bounds, and the registry
// of all fighters. Provides target queries (hostiles/allies) used by
// abilities and enemy AI. Mirrors the `world` object in the JS prototype.
// =============================================================
using UnityEngine;
using System.Collections.Generic;

namespace LF2
{
    public class Arena : MonoBehaviour
    {
        public float GroundY = -180f;
        public float MinX = -560f, MaxX = 560f;

        public readonly List<Fighter> Fighters = new();

        public void Add(Fighter f) { if (!Fighters.Contains(f)) Fighters.Add(f); }
        public void Remove(Fighter f) { Fighters.Remove(f); }

        public List<Fighter> Hostiles(Team team)
        {
            var r = new List<Fighter>();
            foreach (var f in Fighters) if (f.Alive && f.Team != team) r.Add(f);
            return r;
        }

        public List<Fighter> Allies(Team team, Fighter except = null)
        {
            var r = new List<Fighter>();
            foreach (var f in Fighters) if (f.Alive && f.Team == team && f != except) r.Add(f);
            return r;
        }

        public Fighter Nearest(float x, IEnumerable<Fighter> from)
        {
            Fighter best = null; float bd = float.MaxValue;
            foreach (var f in from)
            {
                float d = Mathf.Abs(f.PosX - x);
                if (d < bd) { bd = d; best = f; }
            }
            return best;
        }
    }
}
