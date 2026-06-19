// =============================================================
// Util.cs — runtime sprite generation + hex color parsing.
// Lets the whole game render with zero imported art (colored boxes,
// same as the JS prototype). Swap these for real SpriteRenderers +
// sprite sheets when art arrives.
// =============================================================
using UnityEngine;

namespace LF2
{
    public static class Util
    {
        // Parse "#rrggbb" (or "#rrggbbaa") to a Color; white on failure.
        public static Color Hex(string hex)
        {
            if (ColorUtility.TryParseHtmlString(hex, out var c)) return c;
            return Color.white;
        }

        // A 1x1 white sprite, scaled via transform to whatever rectangle we need.
        static Sprite _unit;
        public static Sprite UnitSprite()
        {
            if (_unit != null) return _unit;
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, Color.white);
            tex.filterMode = FilterMode.Point;
            tex.Apply();
            // pixelsPerUnit = 1 so 1 texture pixel == 1 world unit (== 1 px from JS).
            _unit = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
            return _unit;
        }

        // Make a GameObject that draws a colored box of (w,h) world units.
        public static SpriteRenderer Box(Transform parent, float w, float h, Color color, int order = 0)
        {
            var go = new GameObject("Box");
            if (parent) go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = UnitSprite();
            sr.color = color;
            sr.sortingOrder = order;
            go.transform.localScale = new Vector3(w, h, 1f);
            return sr;
        }
    }
}
