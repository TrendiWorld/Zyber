// =============================================================
// Projectile.cs — straight or arc shot. On contact with a hostile it
// deals damage (+ optional radius splash, launch, stun) and can heal a
// friendly it passes through (Medic's Bio Dart). Ported from
// entities.js Projectile + the projectile_arc behaviour.
// =============================================================
using UnityEngine;

namespace LF2
{
    public class Projectile : MonoBehaviour
    {
        Arena _arena;
        Team _team;
        float _x, _y, _vx, _vy, _gravity;
        float _damage, _knockback, _radius, _launch, _stun, _healAlly;
        float _life;
        SpriteRenderer _sr;

        public void Init(Arena arena, Team team, float x, float y, float vx, float vy,
                         float gravity, float damage, float knockback, Color color,
                         float life = 140, float radius = 0, float launch = 0,
                         float stun = 0, float healAlly = 0)
        {
            _arena = arena; _team = team;
            _x = x; _y = y; _vx = vx; _vy = vy; _gravity = gravity;
            _damage = damage; _knockback = knockback; _radius = radius;
            _launch = launch; _stun = stun; _healAlly = healAlly; _life = life;

            float size = radius > 0 ? 16 : 12;
            _sr = Util.Box(transform, size, size, color, order: 8);
            Sync();
        }

        void FixedUpdate()
        {
            _life--;
            _vy -= _gravity;
            _x += _vx; _y += _vy;

            // arc shots detonate on ground contact
            bool grounded = _gravity > 0 && _y <= 0;

            // hit the first hostile we overlap
            foreach (var f in _arena.Hostiles(_team))
            {
                if (Mathf.Abs(f.PosX - _x) < (f.W * 0.5f + 8) &&
                    Mathf.Abs((f.PosY + f.H * 0.5f) - _y) < (f.H * 0.5f + 8))
                {
                    Detonate(f);
                    return;
                }
            }

            // heal a friendly passed through (e.g. Bio Dart)
            if (_healAlly > 0)
                foreach (var a in _arena.Allies(_team))
                    if (Mathf.Abs(a.PosX - _x) < (a.W * 0.5f + 8)) { a.Heal(_healAlly); _healAlly = 0; }

            if (grounded || _life <= 0 || _x < _arena.MinX || _x > _arena.MaxX)
            {
                if (grounded && _radius > 0) Detonate(null);
                else Destroy(gameObject);
                return;
            }
            Sync();
        }

        void Detonate(Fighter direct)
        {
            if (_radius > 0)
            {
                foreach (var f in _arena.Hostiles(_team))
                    if (Mathf.Abs(f.PosX - _x) < _radius)
                        f.TakeDamage(_damage, _knockback, _x, _launch, _stun);
            }
            else if (direct != null)
            {
                direct.TakeDamage(_damage, _knockback, _x, _launch, _stun);
            }
            Destroy(gameObject);
        }

        void Sync() => transform.position = new Vector3(_x, _arena.GroundY + _y, 0);
    }
}
