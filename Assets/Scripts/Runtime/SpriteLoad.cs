// =============================================================
// SpriteLoad.cs — load a PNG from StreamingAssets/Characters/ into a Sprite
// at runtime. Done in code (not via the importer) so the project needs zero
// manual import-setting wiring — consistent with the rest of the bootstrap.
// Sprites are created at pixelsPerUnit = 1 (1 texel = 1 world px) and cached.
//
// Uses Application.streamingAssetsPath so it works identically in the Editor
// AND in a standalone build (Unity copies StreamingAssets verbatim into the
// player). Art lives in Assets/StreamingAssets/Characters/.
// =============================================================
using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace LF2
{
    public static class SpriteLoad
    {
        static readonly Dictionary<string, Sprite> _cache = new();

        public static Sprite FromCharacterArt(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;
            if (_cache.TryGetValue(fileName, out var cached)) return cached;

            string path = Path.Combine(Application.streamingAssetsPath, "Characters", fileName);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[LF2] sprite not found: {path} — falling back to box.");
                _cache[fileName] = null;
                return null;
            }

            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(path));   // auto-resizes to the PNG
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                       new Vector2(0.5f, 0.5f), 1f);
            _cache[fileName] = sprite;
            return sprite;
        }
    }
}
