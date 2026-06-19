// =============================================================
// PlayerController.cs — drives a Fighter from keyboard input.
// Local co-op: each player gets its own KeyMap (see GameBootstrap).
// Movement (held) is polled in FixedUpdate; action presses are latched
// in Update so GetKeyDown isn't missed between physics steps.
// =============================================================
using UnityEngine;

namespace LF2
{
    public struct KeyMap
    {
        public KeyCode Left, Right, Jump, Attack, Ab1, Ab2;
    }

    public class PlayerController : MonoBehaviour
    {
        public Fighter Fighter;
        public CharacterDef Def;
        KeyMap _k;
        bool _jump, _atk, _a1, _a2;   // latched presses

        public void Setup(Fighter fighter, CharacterDef def, KeyMap keys)
        {
            Fighter = fighter; Def = def; _k = keys;
        }

        void Update()
        {
            // latch edge-triggered actions; cleared when consumed in FixedUpdate
            if (Input.GetKeyDown(_k.Jump)) _jump = true;
            if (Input.GetKeyDown(_k.Attack)) _atk = true;
            if (Input.GetKeyDown(_k.Ab1)) _a1 = true;
            if (Input.GetKeyDown(_k.Ab2)) _a2 = true;
        }

        void FixedUpdate()
        {
            if (Fighter == null || !Fighter.Alive) return;

            float dir = 0;
            if (Input.GetKey(_k.Left)) dir -= 1;
            if (Input.GetKey(_k.Right)) dir += 1;
            Fighter.SetMove(dir);

            if (_jump) { Fighter.Jump(); _jump = false; }
            if (_atk) { AbilitySystem.Execute(Fighter, Def.Ability(AbilitySlot.Attack)); _atk = false; }
            if (_a1)  { AbilitySystem.Execute(Fighter, Def.Ability(AbilitySlot.Ab1)); _a1 = false; }
            if (_a2)  { AbilitySystem.Execute(Fighter, Def.Ability(AbilitySlot.Ab2)); _a2 = false; }
        }
    }
}
