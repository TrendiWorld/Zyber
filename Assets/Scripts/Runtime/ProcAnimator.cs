// =============================================================
// ProcAnimator.cs — procedural ("tween") animation for a single static
// sprite. No frames needed: it reads the Fighter's state each render frame
// and pushes the VisualRoot around — bob while idle, hop while walking,
// stretch in the air, an attack motion that DIFFERS per ability (slash /
// throw / rush), and a recoil on hurt — plus the facing flip.
// Swap to real sprite-sheet clips later without touching gameplay.
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
        float _atk, _hurt, _land;
        float _atkDur = 0.22f;
        string _atkKind = "slash";
        int _lastCueId;
        bool _wasHurt, _wasAir;

        void Start()
        {
            _f = GetComponent<Fighter>();
            _f.MarkAnimated();
            _v = _f.VisualRoot;
            _lastCueId = _f.AnimCueId;
            _phase = (transform.GetInstanceID() % 100) * 0.13f;
        }

        static float Dur(string kind)
        {
            switch (kind)
            {
                case "slash": return 0.20f;
                case "throw": return 0.16f;
                case "rush":  return 0.34f;
                case "beam":  return 0.25f;
                default:      return 0.20f;
            }
        }

        void LateUpdate()
        {
            if (_v == null || !_f.Alive) return;
            float dt = Time.deltaTime;
            _phase += dt;

            // ---- one-shot triggers ----
            if (_f.AnimCueId != _lastCueId)        // a new ability fired
            {
                _lastCueId = _f.AnimCueId;
                _atkKind = _f.AnimCue;
                _atkDur = Dur(_atkKind);
                _atk = 1f;
            }
            if (_f.IsHurt && !_wasHurt) _hurt = 1f;
            if (_f.Grounded && _wasAir) _land = 1f;
            _wasHurt = _f.IsHurt; _wasAir = !_f.Grounded;

            _atk  = Mathf.Max(0, _atk  - dt / _atkDur);
            _hurt = Mathf.Max(0, _hurt - dt / 0.30f);
            _land = Mathf.Max(0, _land - dt / 0.18f);

            // ---- accumulate pose offsets ----
            float ox = 0, oy = 0, rot = 0, sx = 1, sy = 1;
            int dir = _f.Facing;

            if (_hurt > 0)                          // hurt recoil (highest priority)
            {
                float p = _hurt;
                ox += -dir * 16f * p; rot += dir * 10f * p; oy += 3f * p;
            }
            else if (_atk > 0)                       // attack — shape depends on the ability
            {
                float thrust = Mathf.Sin(Mathf.PI * (1f - _atk));     // 0->1->0
                float swing  = Mathf.Sin(Mathf.PI * 2f * (1f - _atk)); // full oscillation
                switch (_atkKind)
                {
                    case "slash":   // snappy lunge + a big arc swing of the blades
                        ox += dir * 40f * thrust; oy += -7f * thrust;
                        sx += 0.20f * thrust; sy += -0.09f * thrust;
                        rot += -dir * 22f * swing;
                        break;
                    case "throw":   // quick short jab forward
                        ox += dir * 16f * thrust; sx += 0.07f * thrust;
                        rot += -dir * 5f * thrust;
                        break;
                    case "rush":    // long forward dash (settles back by the end)
                        ox += dir * 90f * Mathf.SmoothStep(0f, 1f, 1f - _atk);
                        oy += -5f * thrust; sx += 0.16f * thrust; rot += -dir * 12f * thrust;
                        break;
                    case "beam":    // brace back then lean into it
                        ox += -dir * 8f * thrust; sx += 0.05f * thrust;
                        break;
                    default:
                        ox += dir * 22f * thrust; break;
                }
            }
            else if (!_f.Grounded)                  // airborne stretch
            {
                sy += 0.10f; sx += -0.06f; oy += 2f;
            }
            else if (_f.IsMoving)                   // walk: hop + slight forward lean
            {
                oy += Mathf.Abs(Mathf.Sin(_phase * 12f)) * 6f;
                rot += -dir * 5f;
                sx += Mathf.Sin(_phase * 24f) * 0.03f;
            }
            else                                     // idle: gentle breathing bob
            {
                oy += Mathf.Sin(_phase * 2.2f) * 2.2f;
                sy += Mathf.Sin(_phase * 2.2f) * 0.02f;
            }

            if (_land > 0) { sy += -0.12f * _land; sx += 0.10f * _land; }

            float faceSign = (_f.Facing == _f.SpriteNativeFacing) ? 1f : -1f;
            _v.localPosition = new Vector3(ox, oy, 0);
            _v.localScale = new Vector3(sx * faceSign, sy, 1f);
            _v.localRotation = Quaternion.Euler(0, 0, rot);
        }
    }
}
