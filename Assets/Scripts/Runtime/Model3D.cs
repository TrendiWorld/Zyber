// =============================================================
// Model3D.cs — loads a rigged/animated GLB at runtime via glTFast and
// drives it from the Fighter: auto-fits scale to the hurtbox height,
// places feet on the ground, faces the movement direction, and plays the
// baked animation. This is the bridge from the 2D placeholder sprite to a
// real 3D animated character. (Animation state machine comes next.)
// =============================================================
using UnityEngine;
using System.IO;
using System.Threading.Tasks;
using GLTFast;

namespace LF2
{
    public class Model3D : MonoBehaviour
    {
        public Fighter Owner;
        public string FileName;
        public float ScaleMul = 1f;
        public float Yaw = 90f;

        Transform _model;
        Animation _legacy;
        bool _ready;

        public void Configure(Fighter owner, string file, float scaleMul, float yaw)
        {
            Owner = owner; FileName = file; ScaleMul = scaleMul; Yaw = yaw;
        }

        async void Start() { await Load(); }

        async Task Load()
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);
            string uri = path.Contains("://") ? path : "file://" + path;

            var gltf = new GltfImport();
            if (!await gltf.Load(uri)) { Debug.LogError($"[LF2] glTF load failed: {uri}"); return; }

            var holder = new GameObject("Model3DRoot");
            holder.transform.SetParent(Owner.VisualRoot, false);
            if (!await gltf.InstantiateMainSceneAsync(holder.transform)) { Debug.LogError("[LF2] glTF instantiate failed"); return; }
            _model = holder.transform;

            // --- auto-fit: scale combined bounds to ~2x hurtbox height ---
            var rends = holder.GetComponentsInChildren<Renderer>();
            if (rends.Length > 0)
            {
                Bounds b = rends[0].bounds;
                foreach (var r in rends) b.Encapsulate(r.bounds);
                float targetH = Owner.H * 2.0f * ScaleMul;
                float s = b.size.y > 0.0001f ? targetH / b.size.y : 50f;
                holder.transform.localScale = Vector3.one * s;

                // place feet at the VisualRoot origin (ground)
                b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
                float feetLocalY = b.min.y - Owner.VisualRoot.position.y;
                holder.transform.localPosition -= new Vector3(0, feetLocalY, 0);
            }

            _legacy = holder.GetComponentInChildren<Animation>();
            if (_legacy != null)
            {
                _legacy.wrapMode = WrapMode.Loop;
                if (_legacy.clip != null) _legacy.Play();
            }
            _ready = true;

            int rn = holder.GetComponentsInChildren<Renderer>().Length;
            var anim = holder.GetComponentInChildren<Animator>();
            Debug.Log($"[LF2] Model3D '{FileName}' loaded: renderers={rn}, " +
                      $"legacyAnim={(_legacy != null)}, clip={(_legacy?.clip?.name ?? "none")}, " +
                      $"animator={(anim != null)}");
        }

        void LateUpdate()
        {
            if (!_ready || _model == null) return;
            // face the movement direction (rotate the model rather than mirror-scaling)
            _model.localRotation = Quaternion.Euler(0, Yaw * Owner.Facing, 0);
        }
    }
}
