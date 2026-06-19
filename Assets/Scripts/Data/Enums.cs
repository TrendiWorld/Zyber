// =============================================================
// Enums.cs — ability slots and types, ported 1:1 from the JS prototype.
// AbilityType maps to the cases in AbilitySystem.Execute().
// =============================================================
namespace LF2
{
    public enum AbilitySlot { Attack, Ab1, Ab2 }

    public enum AbilityType
    {
        Melee,          // instant arc hit in front
        Projectile,     // straight-flying shot (supports count/spread for bursts)
        ProjectileArc,  // lobbed, gravity-affected (grenades/bombs)
        Aoe,            // burst around the caster (or at an offset)
        HealAoe,        // heals allies in a radius
        BuffSelf,       // applies a timed buff to the caster
        BuffAlly,       // applies a timed buff to the nearest ally
        Beam,           // instant line damage in front
        SpawnTurret,    // deploys an auto-firing turret
        SpawnMine       // deploys a proximity mine
    }

    public enum Team { Players, Enemies }
}
