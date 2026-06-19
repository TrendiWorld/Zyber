// =============================================================
// EnemyController.cs — test-enemy AI, ported from enemies.js.
// Dummy: stands still (+ optional regen). Chaser: walks to nearest
// player and melees. Ranged: kites and fires projectiles.
// =============================================================
using UnityEngine;

namespace LF2
{
    public class EnemyController : MonoBehaviour
    {
        // Global toggle: enemies still move/approach, but deal no damage while off.
        // (Temporary — until their attack art/animations exist.)
        public static bool EnemiesAttack = false;

        public Fighter Fighter;
        public EnemyDef Def;
        float _attackCd;

        public void Setup(Fighter fighter, EnemyDef def) { Fighter = fighter; Def = def; }

        void FixedUpdate()
        {
            if (Fighter == null || !Fighter.Alive) return;
            if (_attackCd > 0) _attackCd--;

            // dummy regen
            if (Def.Ai == EnemyAi.Dummy)
            {
                if (Def.Regen > 0) Fighter.Heal(Def.Regen);
                return;
            }

            var p = Fighter.Arena.Nearest(Fighter.PosX, Fighter.Arena.Hostiles(Fighter.Team));
            if (p == null) { Fighter.SetMove(0); return; }
            float d = p.PosX - Fighter.PosX;
            int face = d >= 0 ? 1 : -1;

            if (Def.Ai == EnemyAi.Chaser)
            {
                if (Mathf.Abs(d) > Def.MeleeRange - 5)
                {
                    Fighter.SetMove(face * 0.5f); // ~half speed approach
                }
                else
                {
                    Fighter.SetMove(0);
                    if (EnemiesAttack && _attackCd <= 0)
                    {
                        _attackCd = 60;
                        foreach (var t in Fighter.Arena.Hostiles(Fighter.Team))
                            if (Mathf.Sign(t.PosX - Fighter.PosX) == face &&
                                Mathf.Abs(t.PosX - Fighter.PosX) <= Def.MeleeRange + t.W * 0.5f)
                                t.TakeDamage(Def.MeleeDamage, Def.MeleeKnockback, Fighter.PosX);
                    }
                }
            }
            else if (Def.Ai == EnemyAi.Ranged)
            {
                float ad = Mathf.Abs(d);
                if (ad < 280) Fighter.SetMove(-face * 0.35f);      // back away
                else if (ad > 460) Fighter.SetMove(face * 0.3f);  // close in
                else Fighter.SetMove(0);

                if (EnemiesAttack && _attackCd <= 0)
                {
                    _attackCd = 75;
                    var go = new GameObject("EnemyShot");
                    go.transform.SetParent(Fighter.Arena.transform, false);
                    go.AddComponent<Projectile>()
                      .Init(Fighter.Arena, Fighter.Team, Fighter.PosX + face * 20,
                            Fighter.H * 0.6f, face * 9, 0, 0, 9, 6, new Color(0.77f, 0.3f, 1f), 140);
                }
            }
        }
    }
}
