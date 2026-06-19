// =============================================================
// Fighter.cs — the combat body shared by players and enemies.
// Custom 60fps sim (FixedUpdate) so all numbers match the JS prototype's
// per-frame units. Holds health/energy, side-view movement + jump,
// knockback, stun, a damage-reduction buff, and per-slot cooldowns.
//
// Visuals: a child "VisualRoot" holds either a colored box OR a real
// sprite. If a ProcAnimator is attached it OWNS VisualRoot's local
// transform (pose); otherwise Fighter applies a simple facing flip.
// =============================================================
using UnityEngine;
using System.Collections.Generic;

namespace LF2
{
    public class Fighter : MonoBehaviour
    {
        public Arena Arena;
        public Team Team;
        public string DisplayName = "Fighter";

        // stats
        public float MaxHp = 100, MaxEnergy = 100, EnergyRegen = 0.18f;
        public float Speed = 5f, JumpImpulse = 17f, Weight = 1f;
        public float W = 58, H = 112;

        // live state
        public float Health, Energy;
        public int Facing = 1;             // +1 right, -1 left
        public float PosX, PosY;           // PosY = height above ground (0 = floor)
        public bool Alive = true;

        // ---- visual ----
        public Transform VisualRoot { get; private set; }
        public int SpriteNativeFacing = 1; // +1 art faces right, -1 art faces left
        SpriteRenderer _rend;              // active renderer (box or sprite)
        Color _baseColor;
        bool _hasAnimator;                 // ProcAnimator owns the pose if true

        // ---- animation-state hooks (read by ProcAnimator) ----
        public bool IsMoving { get; private set; }
        public bool IsBusy => _busy > 0;   // attacking / casting lockout
        public bool IsHurt => _flash > 0;
        public bool Grounded => PosY <= 0.001f;

        // one-shot animation cue fired by an ability; ProcAnimator watches AnimCueId
        public string AnimCue { get; private set; } = "";
        public int AnimCueId { get; private set; }
        public void TriggerAnim(string cue) { AnimCue = cue; AnimCueId++; }

        float _moveVx;                     // from input/AI this frame
        float _kbVx, _vy;                  // knockback + vertical velocity
        float _stun, _busy;                // frames locked out of acting
        float _drFlat, _drTimer;           // damage-reduction buff
        float _flash;                      // hurt flash timer
        readonly Dictionary<AbilitySlot, float> _cd = new();

        const float Gravity = 0.9f;

        public bool CanAct => Alive && _stun <= 0 && _busy <= 0;

        public void Init(Arena arena, Team team, string name, Color color,
                         float w, float h, float maxHp, float maxEnergy, float regen,
                         float speed, float jump, float weight)
        {
            Arena = arena; Team = team; DisplayName = name;
            W = w; H = h; MaxHp = maxHp; MaxEnergy = maxEnergy; EnergyRegen = regen;
            Speed = speed; JumpImpulse = jump; Weight = weight;
            Health = MaxHp; Energy = MaxEnergy;
            _baseColor = color;

            var rootGo = new GameObject("VisualRoot");
            rootGo.transform.SetParent(transform, false);
            VisualRoot = rootGo.transform;

            // default visual: a colored box sitting on the feet
            _rend = Util.Box(VisualRoot, W, H, color, order: 5);
            _rend.transform.localPosition = new Vector3(0, H * 0.5f, 0);
            Sync();
        }

        // Replace the box with a real sprite, auto-fit to the character height.
        public void AttachSprite(string file, float scale, float yOffset, int nativeFacing)
        {
            var sprite = SpriteLoad.FromCharacterArt(file);
            if (sprite == null) return;     // keep the box fallback
            SpriteNativeFacing = nativeFacing;

            if (_rend) Destroy(_rend.gameObject);
            var go = new GameObject("Sprite");
            go.transform.SetParent(VisualRoot, false);
            _rend = go.AddComponent<SpriteRenderer>();
            _rend.sprite = sprite;
            _rend.sortingOrder = 5;

            // the art canvas is mostly transparent padding; aim the full canvas
            // at ~2x the hurtbox height so the visible character lands near H.
            float targetCanvasH = H * 2.2f * scale;
            float s = targetCanvasH / sprite.texture.height;
            go.transform.localScale = new Vector3(s, s, 1f);
            go.transform.localPosition = new Vector3(0, targetCanvasH * 0.5f + yOffset, 0);
        }

        public void MarkAnimated() => _hasAnimator = true;
        public SpriteRenderer Renderer => _rend;
        public Color BaseColor => _baseColor;

        // ---- called by controllers each frame ----
        public void SetMove(float dir)
        {
            if (!CanAct) { _moveVx = 0; IsMoving = false; return; }
            _moveVx = Mathf.Clamp(dir, -1f, 1f) * Speed;
            IsMoving = Mathf.Abs(dir) > 0.01f;
            if (dir != 0) Facing = dir > 0 ? 1 : -1;
        }

        public void Jump() { if (CanAct && Grounded) _vy = JumpImpulse; }
        public void Lock(float frames) => _busy = Mathf.Max(_busy, frames);

        // ---- resources & cooldowns ----
        public bool OnCooldown(AbilitySlot s) => _cd.TryGetValue(s, out var t) && t > 0;
        public void StartCooldown(AbilitySlot s, float frames) => _cd[s] = frames;
        public bool SpendEnergy(float cost)
        {
            if (Energy < cost) return false;
            Energy -= cost; return true;
        }

        // ---- combat ----
        public void TakeDamage(float dmg, float knockback, float fromX, float launch = 0, float stun = 0)
        {
            if (!Alive) return;
            float reduced = _drTimer > 0 ? dmg * (1f - _drFlat) : dmg;
            Health -= reduced;
            int dir = fromX <= PosX ? 1 : -1;
            _kbVx += dir * knockback / Mathf.Max(0.5f, Weight);
            if (launch > 0) _vy = Mathf.Max(_vy, launch);
            if (stun > 0) _stun = Mathf.Max(_stun, stun);
            _flash = 8;
            if (Health <= 0) Die();
        }

        public void Heal(float amount)
        {
            if (!Alive) return;
            Health = Mathf.Min(MaxHp, Health + amount);
        }

        public void ApplyBuff(string stat, float flat, float duration)
        {
            if (stat == "damageReduction") { _drFlat = flat; _drTimer = duration; }
        }

        void Die()
        {
            Alive = false;
            if (_rend) _rend.color = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            Arena.Remove(this);
        }

        // ---- 60fps integration ----
        void FixedUpdate()
        {
            if (!Alive) return;

            if (_stun > 0) _stun--;
            if (_busy > 0) _busy--;
            if (_drTimer > 0) _drTimer--;
            if (_flash > 0) _flash--;
            foreach (var k in new List<AbilitySlot>(_cd.Keys))
                if (_cd[k] > 0) _cd[k]--;

            Energy = Mathf.Min(MaxEnergy, Energy + EnergyRegen);

            _vy -= Gravity;
            PosY += _vy;
            if (PosY <= 0) { PosY = 0; _vy = 0; }

            PosX += _moveVx + _kbVx;
            _kbVx *= 0.85f;
            if (Mathf.Abs(_kbVx) < 0.05f) _kbVx = 0;
            PosX = Mathf.Clamp(PosX, Arena.MinX, Arena.MaxX);
            _moveVx = 0;

            Sync();
        }

        void Sync()
        {
            transform.position = new Vector3(PosX, Arena ? Arena.GroundY + PosY : PosY, 0);

            // hurt flash: white for boxes, red for sprites (white is invisible on a sprite)
            if (_rend && Alive)
            {
                if (_flash > 0) _rend.color = _rend.sprite ? new Color(1f, 0.45f, 0.45f) : Color.white;
                else            _rend.color = _rend.sprite ? Color.white : _baseColor;
            }

            // if no animator, apply a simple facing flip ourselves
            if (!_hasAnimator && VisualRoot)
            {
                var s = VisualRoot.localScale;
                s.x = Mathf.Abs(s.x) * Facing;
                VisualRoot.localScale = s;
            }
        }
    }
}
