// =============================================================
// EnemyDef.cs — a test enemy archetype. AI behaviour lives in
// EnemyController (selected by AiKind), mirroring enemies.js.
// =============================================================
namespace LF2
{
    public enum EnemyAi { Dummy, Chaser, Ranged }

    public class EnemyDef
    {
        public string Id;
        public string Name;
        public string Color;
        public EnemyAi Ai;

        public float W = 56f;
        public float H = 108f;
        public float Weight = 1f;
        public float Hp = 90f;
        public float Regen = 0f;

        // melee profile (for chaser)
        public float MeleeDamage = 8f;
        public float MeleeRange = 60f;
        public float MeleeKnockback = 7f;
    }
}
