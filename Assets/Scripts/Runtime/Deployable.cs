// =============================================================
// Deployable.cs — turret (auto-fires at nearest hostile) and mine
// (detonates when a hostile enters its trigger radius). Ported from
// the spawn_turret / spawn_mine ability behaviours.
// =============================================================
using UnityEngine;

namespace LF2
{
    public class Deployable : MonoBehaviour
    {
        Arena _arena;
        Team _team;
        bool _isMine;
        AbilityDef _ab;
        float _x, _life, _fireTimer;
        SpriteRenderer _sr;

        public static void Spawn(Arena arena, Team team, float x, bool isMine, AbilityDef ab, Color color)
        {
            var go = new GameObject(isMine ? "Mine" : "Turret");
            go.transform.SetParent(arena.transform, false);
            var d = go.AddComponent<Deployable>();
            d._arena = arena; d._team = team; d._isMine = isMine; d._ab = ab;
            d._x = x; d._life = ab.Life;
            float s = isMine ? 22 : 34;
            d._sr = Util.Box(go.transform, s, isMine ? 14 : s, color, order: 4);
            go.transform.position = new Vector3(x, arena.GroundY + (isMine ? 7 : 17), 0);
        }

        void FixedUpdate()
        {
            _life--;
            if (_life <= 0) { Destroy(gameObject); return; }

            var target = _arena.Nearest(_x, _arena.Hostiles(_team));
            if (target == null) return;

            if (_isMine)
            {
                if (Mathf.Abs(target.PosX - _x) < _ab.Trigger)
                {
                    foreach (var f in _arena.Hostiles(_team))
                        if (Mathf.Abs(f.PosX - _x) < _ab.Radius)
                            f.TakeDamage(_ab.Damage, _ab.Knockback, _x, 8, 0);
                    Fx.Ring(_arena, _x, 10, _ab.Radius, new Color(1f, 0.5f, 0.2f, 0.6f));
                    Destroy(gameObject);
                }
            }
            else // turret
            {
                _fireTimer--;
                if (_fireTimer <= 0)
                {
                    _fireTimer = _ab.FireRate;
                    int dir = target.PosX >= _x ? 1 : -1;
                    var go = new GameObject("TurretShot");
                    go.transform.SetParent(_arena.transform, false);
                    go.AddComponent<Projectile>()
                      .Init(_arena, _team, _x, 28, dir * 12, 0, 0, _ab.Damage, 3,
                            new Color(0.4f, 0.9f, 1f), 120);
                }
            }
        }
    }
}
