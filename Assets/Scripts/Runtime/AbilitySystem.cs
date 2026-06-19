// =============================================================
// AbilitySystem.cs — executes an AbilityDef for a caster. This is the
// port of executeAbility() from entities.js: one branch per AbilityType.
// Handles cost, cooldown, facing, and the action lock (busy frames).
// =============================================================
using UnityEngine;

namespace LF2
{
    public static class AbilitySystem
    {
        // Returns true if the ability actually fired.
        public static bool Execute(Fighter caster, AbilityDef ab)
        {
            if (ab == null || !caster.CanAct) return false;
            if (caster.OnCooldown(ab.Slot)) return false;
            if (!caster.SpendEnergy(ab.Cost)) return false;

            caster.StartCooldown(ab.Slot, ab.Cooldown);
            caster.Lock(14); // generic cast/swing lockout (frames)

            var arena = caster.Arena;
            float ox = caster.PosX + caster.Facing * 30;     // muzzle/strike origin
            float oy = caster.PosY + caster.H * 0.6f;        // mid-body height
            Color col = Util.Hex(ab.Color);

            switch (ab.Type)
            {
                case AbilityType.Melee:
                {
                    foreach (var f in arena.Hostiles(caster.Team))
                    {
                        float dx = f.PosX - caster.PosX;
                        if (Mathf.Sign(dx) != caster.Facing && Mathf.Abs(dx) > 4) continue;
                        if (Mathf.Abs(dx) <= ab.Range + f.W * 0.5f)
                            f.TakeDamage(ab.Damage, ab.Knockback, caster.PosX, ab.Launch, ab.Stun);
                    }
                    break;
                }

                case AbilityType.Projectile:
                {
                    int n = Mathf.Max(1, ab.Count);
                    for (int i = 0; i < n; i++)
                    {
                        float spreadDeg = ab.Spread * (i - (n - 1) / 2f);
                        float rad = spreadDeg * Mathf.Deg2Rad;
                        float vx = Mathf.Cos(rad) * ab.Speed * caster.Facing;
                        float vy = Mathf.Sin(rad) * ab.Speed;
                        Spawn(arena, caster.Team, ox, oy, vx, vy, 0,
                              ab.Damage, ab.Knockback, col, 140, ab.Radius, ab.Launch, ab.Stun, ab.HealAlly);
                    }
                    break;
                }

                case AbilityType.ProjectileArc:
                {
                    float vx = ab.Speed > 0 ? ab.Speed * caster.Facing : 9 * caster.Facing;
                    Spawn(arena, caster.Team, ox, oy, vx, ab.Lift, ab.Gravity,
                          ab.Damage, ab.Knockback, col.Equals(Color.white) ? new Color(1f, 0.6f, 0.2f) : col,
                          240, ab.Radius, ab.Launch, ab.Stun);
                    break;
                }

                case AbilityType.Aoe:
                {
                    float cx = caster.PosX + caster.Facing * ab.Offset;
                    foreach (var f in arena.Hostiles(caster.Team))
                        if (Mathf.Abs(f.PosX - cx) < ab.Radius)
                            f.TakeDamage(ab.Damage, ab.Knockback, cx, ab.Launch, ab.Stun);
                    Fx.Ring(arena, cx, caster.PosY + 10, ab.Radius, new Color(0.7f, 0.85f, 1f, 0.5f));
                    break;
                }

                case AbilityType.HealAoe:
                {
                    foreach (var a in arena.Allies(caster.Team, caster)) // teammates (excludes caster)
                        if (Mathf.Abs(a.PosX - caster.PosX) < ab.Radius) a.Heal(ab.Heal);
                    caster.Heal(ab.Heal);                                 // caster always healed once
                    Fx.Ring(arena, caster.PosX, caster.PosY + 10, ab.Radius, new Color(0.4f, 1f, 0.6f, 0.5f));
                    break;
                }

                case AbilityType.BuffSelf:
                    caster.ApplyBuff(ab.BuffStat, ab.BuffFlat, ab.Duration);
                    break;

                case AbilityType.BuffAlly:
                {
                    var ally = arena.Nearest(caster.PosX, arena.Allies(caster.Team, caster));
                    (ally ?? caster).ApplyBuff(ab.BuffStat, ab.BuffFlat, ab.Duration);
                    break;
                }

                case AbilityType.Beam:
                {
                    foreach (var f in arena.Hostiles(caster.Team))
                    {
                        float dx = f.PosX - caster.PosX;
                        if (Mathf.Sign(dx) == caster.Facing && Mathf.Abs(dx) <= ab.Range)
                            f.TakeDamage(ab.Damage, ab.Knockback, caster.PosX, 0, ab.Stun);
                    }
                    Fx.Beam(arena, caster.PosX, oy, caster.Facing * ab.Range, col);
                    break;
                }

                case AbilityType.SpawnTurret:
                    Deployable.Spawn(arena, caster.Team, caster.PosX + caster.Facing * 40,
                                     isMine: false, ab, Util.Hex(caster_color(caster)));
                    break;

                case AbilityType.SpawnMine:
                    Deployable.Spawn(arena, caster.Team, caster.PosX + caster.Facing * 40,
                                     isMine: true, ab, new Color(1f, 0.5f, 0.2f));
                    break;
            }
            return true;
        }

        static string caster_color(Fighter f) => "#4ad6ff";

        static void Spawn(Arena arena, Team team, float x, float y, float vx, float vy, float gravity,
                          float damage, float knockback, Color color, float life,
                          float radius, float launch, float stun, float healAlly = 0)
        {
            var go = new GameObject("Projectile");
            go.transform.SetParent(arena.transform, false);
            go.AddComponent<Projectile>()
              .Init(arena, team, x, y, vx, vy, gravity, damage, knockback, color, life, radius, launch, stun, healAlly);
        }
    }
}
