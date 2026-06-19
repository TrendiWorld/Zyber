// =============================================================
// CharacterDef.cs — one playable role and its three abilities.
// Mirrors a block in characters.js.
// =============================================================
using System.Collections.Generic;

namespace LF2
{
    public class CharacterDef
    {
        public string Id;
        public string Name;
        public string Role;
        public string Color;
        public string Blurb;

        // stats (same units as the JS prototype: px/frame, frames, etc.)
        public float MaxHp;
        public float MaxEnergy;
        public float EnergyRegen = 0.18f;
        public float Speed;
        public float Jump = 17f;
        public float Weight = 1f;

        // hurtbox size in px
        public float HurtW = 58f;
        public float HurtH = 112f;

        // --- art (optional). If SpriteFile is set, Fighter renders this PNG
        // (from Assets/Art/Characters/) instead of a colored box. ---
        public string SpriteFile;           // e.g. "ninja_cutout.png"; null = colored box
        public float SpriteScale = 1f;      // multiplier on auto-fit height
        public float SpriteYOffset = 0f;    // nudge sprite up/down (px)
        public int SpriteNativeFacing = 1;  // +1 if the art faces right, -1 if it faces left

        public List<AbilityDef> Abilities = new List<AbilityDef>();

        public AbilityDef Ability(AbilitySlot slot)
            => Abilities.Find(a => a.Slot == slot);
    }
}
