// =============================================================
// Roster.cs — THE ROSTER, ported 1:1 from lf2-game/src/data/characters.js
// and enemies.js. This is the "space to create characters".
//
// To add a character: copy a block in Characters(), change numbers/abilities.
// To add an ability: set its Slot, Type (see AbilitySystem), and the params
// that type reads.
// =============================================================
using System.Collections.Generic;

namespace LF2
{
    public static class Roster
    {
        // -------- helper builders (keep the data terse like the JS) --------
        static AbilityDef A(string id, AbilitySlot slot, AbilityType type, string clip)
            => new AbilityDef { Id = id, Slot = slot, Type = type, Clip = clip };

        public static List<CharacterDef> Characters()
        {
            var list = new List<CharacterDef>();

            // ----------------------------------------------------- NINJA (DPS)
            // First real art-driven character. Uses the Midjourney cutout.
            list.Add(new CharacterDef {
                Id = "ninja", Name = "Ninja", Role = "Assassin", Color = "#e23b3b",
                MaxHp = 165, MaxEnergy = 120, Speed = 6.2f, Weight = 0.85f, Jump = 18,
                HurtW = 56, HurtH = 118,
                SpriteFile = "ninja_cutout.png", SpriteScale = 1f, SpriteYOffset = 0, SpriteNativeFacing = -1,
                Model3DFile = "ninja_v2.glb", Model3DScale = 1f, Model3DYaw = 90f,
                Blurb = "High-speed assassin mech. Twin energy katanas, in and out before you react.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="nj_slash", Name="Twin Slash", Slot=AbilitySlot.Attack, Type=AbilityType.Melee, Clip="attack",
                        Damage=12, Range=82, Knockback=6, Cost=0, Cooldown=12 },
                    new AbilityDef { Id="nj_shuriken", Name="Shuriken", Slot=AbilitySlot.Ab1, Type=AbilityType.Projectile, Clip="cast1",
                        Damage=8, Count=3, Spread=6, Speed=18, Knockback=3, Color="#ff5a5a", Cost=18, Cooldown=50 },
                    new AbilityDef { Id="nj_rush", Name="Blade Rush", Slot=AbilitySlot.Ab2, Type=AbilityType.Aoe, Clip="cast2",
                        Damage=26, Radius=90, Offset=60, Knockback=10, Launch=8, Stun=15, Cost=35, Cooldown=110 },
                }
            });

            // ----------------------------------------------------- MEDIC
            list.Add(new CharacterDef {
                Id = "medic", Name = "Medic", Role = "Medic", Color = "#3ad29f",
                MaxHp = 180, MaxEnergy = 120, Speed = 5.2f, Weight = 0.9f,
                Blurb = "Keeps the squad alive. Weak alone, deadly as support.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="med_staff", Slot=AbilitySlot.Attack, Type=AbilityType.Melee, Clip="attack",
                        Damage=8, Range=70, Knockback=5, Cost=0, Cooldown=14 },
                    new AbilityDef { Id="med_pulse", Name="Heal Pulse", Slot=AbilitySlot.Ab1, Type=AbilityType.HealAoe, Clip="cast1",
                        Heal=45, Radius=220, Cost=40, Cooldown=150 },
                    new AbilityDef { Id="med_dart", Name="Bio Dart", Slot=AbilitySlot.Ab2, Type=AbilityType.Projectile, Clip="cast2",
                        Damage=12, HealAlly=30, Speed=14, Knockback=3, Color="#7CFC00", Cost=20, Cooldown=45 },
                }
            });

            // ----------------------------------------------------- TANK
            list.Add(new CharacterDef {
                Id = "tank", Name = "Tank", Role = "Tank", Color = "#5a8dff",
                MaxHp = 320, MaxEnergy = 100, Speed = 4.0f, Weight = 1.8f, Jump = 14,
                HurtW = 70, HurtH = 124,
                Blurb = "Soaks damage, controls space, throws bodies around.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="tank_bash", Slot=AbilitySlot.Attack, Type=AbilityType.Melee, Clip="attack",
                        Damage=14, Range=78, Knockback=12, Cost=0, Cooldown=20 },
                    new AbilityDef { Id="tank_guard", Name="Fortify", Slot=AbilitySlot.Ab1, Type=AbilityType.BuffSelf, Clip="cast1",
                        BuffStat="damageReduction", BuffFlat=0.6f, Duration=240, Cost=30, Cooldown=300 },
                    new AbilityDef { Id="tank_slam", Name="Ground Slam", Slot=AbilitySlot.Ab2, Type=AbilityType.Aoe, Clip="cast2",
                        Damage=22, Radius=150, Knockback=18, Launch=12, Stun=25, Offset=40, Cost=45, Cooldown=180 },
                }
            });

            // ----------------------------------------------------- SHOOTER
            list.Add(new CharacterDef {
                Id = "shooter", Name = "Shooter", Role = "Shooter", Color = "#ffb74d",
                MaxHp = 160, MaxEnergy = 110, Speed = 5.6f,
                Blurb = "Ranged DPS. Pokes from afar, lobs grenades to flush cover.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="sh_shot", Name="Pistol", Slot=AbilitySlot.Attack, Type=AbilityType.Projectile, Clip="cast1",
                        Damage=9, Speed=16, Knockback=4, Color="#fff1a8", Cost=0, Cooldown=16 },
                    new AbilityDef { Id="sh_burst", Name="Burst Fire", Slot=AbilitySlot.Ab1, Type=AbilityType.Projectile, Clip="cast1",
                        Damage=7, Count=3, Spread=2.2f, Speed=15, Knockback=3, Color="#ffd166", Cost=25, Cooldown=70 },
                    new AbilityDef { Id="sh_nade", Name="Grenade", Slot=AbilitySlot.Ab2, Type=AbilityType.ProjectileArc, Clip="cast2",
                        Damage=26, Radius=110, Knockback=14, Launch=10, Lift=11, Gravity=0.5f, Cost=35, Cooldown=140 },
                }
            });

            // ----------------------------------------------------- MAGE
            list.Add(new CharacterDef {
                Id = "mage", Name = "Mage", Role = "Mage", Color = "#b06bff",
                MaxHp = 150, MaxEnergy = 160, Speed = 5.0f,
                Blurb = "Burst magic damage and crowd control. Glass cannon.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="mg_bolt", Name="Arcane Bolt", Slot=AbilitySlot.Attack, Type=AbilityType.Projectile, Clip="cast1",
                        Damage=11, Speed=14, Knockback=4, Color="#d6a8ff", Cost=0, Cooldown=22 },
                    new AbilityDef { Id="mg_fire", Name="Fireball", Slot=AbilitySlot.Ab1, Type=AbilityType.Projectile, Clip="cast2",
                        Damage=34, Radius=120, Speed=10, Knockback=12, Color="#ff7043", Cost=45, Cooldown=120 },
                    new AbilityDef { Id="mg_frost", Name="Frost Nova", Slot=AbilitySlot.Ab2, Type=AbilityType.Aoe, Clip="cast2",
                        Damage=14, Radius=170, Knockback=6, Stun=45, Cost=40, Cooldown=160 },
                }
            });

            // ----------------------------------------------------- ROBOTICS
            list.Add(new CharacterDef {
                Id = "robotics", Name = "Robotics", Role = "Robotics", Color = "#4ad6ff",
                MaxHp = 200, MaxEnergy = 130, Speed = 4.6f,
                Blurb = "Deploys turrets and burns targets down with a laser.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="rb_punch", Slot=AbilitySlot.Attack, Type=AbilityType.Melee, Clip="attack",
                        Damage=11, Range=72, Knockback=8, Cost=0, Cooldown=18 },
                    new AbilityDef { Id="rb_turret", Name="Deploy Turret", Slot=AbilitySlot.Ab1, Type=AbilityType.SpawnTurret, Clip="cast1",
                        Damage=6, FireRate=35, Life=540, Cost=45, Cooldown=260 },
                    new AbilityDef { Id="rb_laser", Name="Laser Beam", Slot=AbilitySlot.Ab2, Type=AbilityType.Beam, Clip="cast2",
                        Damage=30, Range=700, Knockback=5, Stun=12, Cost=50, Cooldown=150 },
                }
            });

            // ----------------------------------------------------- TECH
            list.Add(new CharacterDef {
                Id = "tech", Name = "Tech", Role = "Tech", Color = "#ff5e9c",
                MaxHp = 175, MaxEnergy = 140, Speed = 5.4f,
                Blurb = "Disruption specialist: stuns, EMPs, and shields allies.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="tc_baton", Slot=AbilitySlot.Attack, Type=AbilityType.Melee, Clip="attack",
                        Damage=9, Range=66, Knockback=6, Stun=22, Cost=0, Cooldown=18 },
                    new AbilityDef { Id="tc_emp", Name="EMP Burst", Slot=AbilitySlot.Ab1, Type=AbilityType.Aoe, Clip="cast1",
                        Damage=10, Radius=200, Knockback=4, Stun=60, Offset=0, Cost=40, Cooldown=170 },
                    new AbilityDef { Id="tc_shield", Name="Shield Drone", Slot=AbilitySlot.Ab2, Type=AbilityType.BuffAlly, Clip="cast2",
                        BuffStat="damageReduction", BuffFlat=0.5f, Duration=300, Cost=35, Cooldown=220 },
                }
            });

            // ----------------------------------------------------- TINKER
            list.Add(new CharacterDef {
                Id = "tinker", Name = "Tinker", Role = "Tinker", Color = "#ffd23f",
                MaxHp = 185, MaxEnergy = 120, Speed = 5.2f,
                Blurb = "Zone control with bombs and mines. Sets traps, denies ground.",
                Abilities = new List<AbilityDef> {
                    new AbilityDef { Id="tk_wrench", Slot=AbilitySlot.Attack, Type=AbilityType.Melee, Clip="attack",
                        Damage=10, Range=68, Knockback=7, Cost=0, Cooldown=16 },
                    new AbilityDef { Id="tk_bomb", Name="Throw Bomb", Slot=AbilitySlot.Ab1, Type=AbilityType.ProjectileArc, Clip="cast2",
                        Damage=28, Radius=120, Knockback=14, Launch=11, Lift=12, Gravity=0.55f, Cost=32, Cooldown=120 },
                    new AbilityDef { Id="tk_mine", Name="Deploy Mine", Slot=AbilitySlot.Ab2, Type=AbilityType.SpawnMine, Clip="cast1",
                        Damage=30, Radius=110, Trigger=55, Life=700, Cost=30, Cooldown=150 },
                }
            });

            return list;
        }

        // -------------------------------------------------- ENEMIES
        public static List<EnemyDef> Enemies()
        {
            return new List<EnemyDef> {
                new EnemyDef { Id="dummy", Name="Training Dummy", Color="#8892b0",
                    Ai=EnemyAi.Dummy, W=60, H=116, Weight=3, Hp=250, Regen=1.5f },
                new EnemyDef { Id="chaser", Name="Chaser", Color="#e85d75",
                    Ai=EnemyAi.Chaser, W=56, H=108, Weight=1, Hp=90,
                    MeleeDamage=8, MeleeRange=60, MeleeKnockback=7 },
                new EnemyDef { Id="ranged", Name="Ranged Bot", Color="#c44dff",
                    Ai=EnemyAi.Ranged, W=54, H=106, Weight=1, Hp=70 },
            };
        }

        public static CharacterDef GetCharacter(string id)
            => Characters().Find(c => c.Id == id);
    }
}
