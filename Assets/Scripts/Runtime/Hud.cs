// =============================================================
// Hud.cs — IMGUI overlay: health/energy bars for every fighter plus a
// controls legend. IMGUI is used so no Canvas/UI prefab wiring is needed.
// =============================================================
using UnityEngine;

namespace LF2
{
    public class Hud : MonoBehaviour
    {
        public Arena Arena;
        Texture2D _white;
        GUIStyle _label;

        void Awake()
        {
            _white = new Texture2D(1, 1);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
        }

        void Bar(float x, float y, float w, float h, float frac, Color fill, Color bg)
        {
            GUI.color = bg; GUI.DrawTexture(new Rect(x, y, w, h), _white);
            GUI.color = fill; GUI.DrawTexture(new Rect(x, y, w * Mathf.Clamp01(frac), h), _white);
            GUI.color = Color.white;
        }

        void OnGUI()
        {
            if (Arena == null) return;
            _label ??= new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold };

            float y = 12;
            foreach (var f in Arena.Fighters)
            {
                if (!f.Alive) continue;
                string side = f.Team == Team.Players ? "►" : "✖";
                GUI.Label(new Rect(14, y - 2, 240, 18), $"{side} {f.DisplayName}", _label);
                Bar(150, y, 180, 12, f.Health / f.MaxHp,
                    f.Team == Team.Players ? new Color(0.3f, 0.85f, 0.45f) : new Color(0.9f, 0.35f, 0.4f),
                    new Color(0, 0, 0, 0.5f));
                if (f.Team == Team.Players)
                    Bar(150, y + 14, 180, 6, f.Energy / f.MaxEnergy, new Color(0.35f, 0.6f, 1f), new Color(0, 0, 0, 0.5f));
                y += 34;
            }

            GUI.Label(new Rect(14, Screen.height - 70, 900, 20),
                "P1: A/D move · W jump · F attack · G/H abilities      P2: ←/→ move · ↑ jump · . attack · ,/M abilities", _label);
            GUI.Label(new Rect(14, Screen.height - 48, 900, 20),
                "R reset · [ ] cycle P1 character", _label);
        }
    }
}
