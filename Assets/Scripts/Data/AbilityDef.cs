// =============================================================
// AbilityDef.cs — a plain data class describing one ability.
// Mirrors an ability object from characters.js. All gameplay numbers
// live here; AbilitySystem reads them by Type. Not a ScriptableObject yet
// (kept as a POCO so the whole roster builds in code with no editor
// authoring) — easy to convert to a ScriptableObject later for designers.
// =============================================================
namespace LF2
{
    public class AbilityDef
    {
        public string Id;
        public string Name;
        public AbilitySlot Slot;
        public AbilityType Type;
        public string Clip = "attack";   // which animation clip plays

        // --- costs / timing (in frames, 60 = 1s) ---
        public float Cost;
        public float Cooldown;

        // --- damage / impact ---
        public float Damage;
        public float Knockback;
        public float Range;      // melee reach
        public float Radius;     // aoe / explosion radius
        public float Stun;       // frames of stun applied
        public float Launch;     // upward pop on hit
        public float Offset;     // horizontal offset of an aoe from caster

        // --- projectile params ---
        public float Speed;
        public int Count = 1;    // bullets per shot (burst)
        public float Spread;     // degrees of spread between burst bullets
        public float Lift;       // initial upward velocity for arc shots
        public float Gravity;    // gravity for arc shots

        // --- support params ---
        public float Heal;       // heal_aoe amount
        public float HealAlly;   // bonus heal when a projectile hits an ally

        // --- buff params ---
        public string BuffStat;  // e.g. "damageReduction"
        public float BuffFlat;
        public float Duration;

        // --- spawn params (turret / mine) ---
        public float FireRate;
        public float Life;
        public float Trigger;    // mine trigger radius

        // --- visuals ---
        public string Color = "#ffffff";
    }
}
