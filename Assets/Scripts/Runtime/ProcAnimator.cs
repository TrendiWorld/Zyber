// =============================================================
// ProcAnimator.cs — procedural ("tween") animation for a single static
// sprite. No frames needed: it reads the Fighter's state each render frame
// and pushes the VisualRoot around — bob while idle, hop while walking,
// stretch in the air, a forward lunge on attack, a recoil on hurt — plus
// the facing flip. This is the free path while autosprite is unavailable;
// swap to real sprite-sheet clips later without touching gameplay.
// =============================================================
using UnityEngine;

namespace LF2
{
    [RequireComponent(typeof(Fighter))]
    public class ProcAnimator : MonoBehaviour
    {
        Fighter _f;
        Transform _v;
        float _phase;          // free-running clock for cyclic motion
        float _atk, _hurt;     // one-shot timers (1 -> 0)
        bool _wasBusy, _wasHurt, _wasAir;
        float _land;           // landing squash timer

        void Start()
        {
            _f = GetComponent<Fighter>();
            _f.MarkAnimated();
            _v = _f.VisualRoot;
            // desync each character's idle bob so they don't move in lockstep
            _phase = (transform.GetInstanceID() % 100) * 0.13f;
        }

        void LateUpdate()
        {
            if (_v == null || !_f.Alive) return;
            float dt = Time.deltaTime;
            _phase += dt;

            // ---- edge-detect one-shot states ----
            if (_f.IsBusy && !_wasBusy) _atk = 1f;
            if (_f.IsHurt && !_wasHurt) _hurt = 1f;
            if (_f.Grounded && _wasAir) _land = 1f;
            _wasBusy = _f.IsBusy; _wasHurt = _f.IsHurt; _wasAir = !_f.Grounded;

            _atk  = Mathf.Max(0, _atk  - dt / 0.22f);  // ~0.22s slash
            _hurt = Mathf.Max(0, _hurt - dt / 0.30f);
            _land = Mathf.Max(0, _land - dt / 0.18f);

            // ---- accumulate pose offsets ----
            float ox = 0, oy = 0, rot = 0, sx = 1, sy = 1;
            int dir = _f.Facing; // world direction the fighter looks

            if (_hurt > 0)                       // hurt recoil (highest priority)
            {
                float p = _hurt;
                ox += -dir * 16f * p;
                rot += dir * 10f * p;
                oy += 3f * p;
            }
            else if (_atk > 0)                    // attack lunge: thrust out then back
            {
                float thrust = Mathf.Sin(Mathf.PI * (1f - _atk)); // 0->1->0
                ox += dir * 34f * thrust;
                oy += -6f * thrust;
                sx += 0.14f * thrust;
                sy += -0.06f * thrust;
                rot += -dir * 6f * thrust;
            }
            else if (!_f.Grounded)               // airborne stretch
            {
                sy += 0.10f; sx += -0.06f;
                oy += 2f;
            }
            else if (_f.IsMoving)                // walk: hop + slight forward lean
            {
                oy += Mathf.Abs(Mathf.Sin(_phase * 12f)) * 6f;
                rot += -dir * 5f;
                sx += Mathf.Sin(_phase * 24f) * 0.03f;
            }
            else                                  // idle: gentle breathing bob
            {
                oy += Mathf.Sin(_phase * 2.2f) * 2.2f;
                sy += Mathf.Sin(_phase * 2.2f) * 0.02f;
            }

            if (_land > 0) { sy += -0.12f * _land; sx += 0.10f * _land; } // landing squash

            // ---- facing flip: art's native facing vs current facing ----
            float faceSign = (_f.Facing == _f.SpriteNativeFacing) ? 1f : -1f;

            _v.localPosition = new Vector3(ox, oy, 0);
            _v.localScale = new Vector3(sx * faceSign, sy, 1f);
            _v.localRotation = Quaternion.Euler(0, 0, rot);
        }
    }
}
