using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ClawbearGames
{
    [ExecuteAlways]
    public class StylizedBombBuilder : MonoBehaviour
    {
        [Header("Bomb Shape")]
        [SerializeField, Min(0.2f)] private float bodyRadius = 0.5f;
        [SerializeField, Range(8, 12)] private int spikeCount = 10;
        [SerializeField, Min(0.05f)] private float spikeLength = 0.2f;
        [SerializeField, Min(0.02f)] private float spikeBaseRadius = 0.1f;
        [SerializeField, Min(0.0f)] private float spikeTipRadius = 0.02f;

        [Header("Fuse")]
        [SerializeField, Min(0.05f)] private float capRadius = 0.12f;
        [SerializeField, Min(0.02f)] private float capHeight = 0.05f;
        [SerializeField, Min(0.05f)] private float fuseLength = 0.18f;
        [SerializeField, Min(0.01f)] private float fuseRadius = 0.025f;

        [Header("Materials")]
        [SerializeField] private Material bodyMaterial;
        [SerializeField] private Material spikeMaterial;
        [SerializeField] private Material fuseMaterial;

        [Header("Editor")]
        [SerializeField] private bool rebuildOnValidate = true;

        private const string VisualRootName = "Bomb_Visual";
        private Mesh cachedSpikeMesh;

        private void Reset()
        {
            Rebuild();
        }

        private void OnValidate()
        {
            if (!rebuildOnValidate)
            {
                return;
            }

            Rebuild();
        }

        [ContextMenu("Rebuild Bomb")]
        public void Rebuild()
        {
#if UNITY_EDITOR
            if (ShouldSkipEditorRebuild())
            {
                return;
            }
#endif
            TryAssignDefaultMaterial();

            Transform visualRoot = GetOrCreateVisualRoot();
            ClearChildren(visualRoot);

            CreateBody(visualRoot);
            CreateSpikes(visualRoot);
            CreateFuse(visualRoot);
        }

#if UNITY_EDITOR
        private bool ShouldSkipEditorRebuild()
        {
            if (Application.isPlaying)
            {
                return false;
            }

            return PrefabUtility.IsPartOfPrefabAsset(gameObject);
        }
#endif

        private Transform GetOrCreateVisualRoot()
        {
            Transform visualRoot = transform.Find(VisualRootName);
            if (visualRoot != null)
            {
                return visualRoot;
            }

            GameObject visualRootObject = new GameObject(VisualRootName);
            visualRootObject.transform.SetParent(transform, false);
            return visualRootObject.transform;
        }

        private static void ClearChildren(Transform root)
        {
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                DestroySafe(root.GetChild(i).gameObject);
            }
        }

        private void CreateBody(Transform parent)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "Body";
            body.transform.SetParent(parent, false);
            body.transform.localScale = Vector3.one * (bodyRadius * 2f);

            ApplyMaterial(body, bodyMaterial);
            RemoveCollider(body);
        }

        private void CreateSpikes(Transform parent)
        {
            Mesh spikeMesh = GetSpikeMesh();
            IReadOnlyList<Vector3> spikeDirections = GetSpikeDirections(spikeCount);

            for (int i = 0; i < spikeDirections.Count; i++)
            {
                Vector3 direction = spikeDirections[i].normalized;

                GameObject spike = new GameObject($"Spike_{i:00}");
                spike.transform.SetParent(parent, false);
                spike.transform.localPosition = direction * (bodyRadius * 0.95f);
                spike.transform.localRotation = Quaternion.LookRotation(direction);

                MeshFilter filter = spike.AddComponent<MeshFilter>();
                filter.sharedMesh = spikeMesh;

                MeshRenderer renderer = spike.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = spikeMaterial != null ? spikeMaterial : bodyMaterial;
            }
        }

        private void CreateFuse(Transform parent)
        {
            GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cap.name = "Fuse_Cap";
            cap.transform.SetParent(parent, false);
            cap.transform.localPosition = Vector3.up * (bodyRadius * 0.95f);
            cap.transform.localScale = new Vector3(capRadius * 2f, capHeight * 0.5f, capRadius * 2f);
            ApplyMaterial(cap, bodyMaterial);
            RemoveCollider(cap);

            GameObject fuse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fuse.name = "Fuse";
            fuse.transform.SetParent(parent, false);
            fuse.transform.localPosition = cap.transform.localPosition + new Vector3(0f, capHeight + fuseLength * 0.35f, 0.02f);
            fuse.transform.localRotation = Quaternion.Euler(18f, 0f, -12f);
            fuse.transform.localScale = new Vector3(fuseRadius * 2f, fuseLength * 0.5f, fuseRadius * 2f);
            ApplyMaterial(fuse, fuseMaterial != null ? fuseMaterial : spikeMaterial);
            RemoveCollider(fuse);
        }

        private Mesh GetSpikeMesh()
        {
            if (cachedSpikeMesh != null)
            {
                return cachedSpikeMesh;
            }

            cachedSpikeMesh = CreateTruncatedConeMesh(8, spikeBaseRadius, spikeTipRadius, spikeLength);
            cachedSpikeMesh.name = "StylizedBombSpike";
            return cachedSpikeMesh;
        }

        private static Mesh CreateTruncatedConeMesh(int sides, float baseRadius, float tipRadius, float length)
        {
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();

            for (int i = 0; i < sides; i++)
            {
                float angle = (Mathf.PI * 2f * i) / sides;
                float x = Mathf.Cos(angle);
                float z = Mathf.Sin(angle);
                vertices.Add(new Vector3(x * baseRadius, 0f, z * baseRadius));
                vertices.Add(new Vector3(x * tipRadius, length, z * tipRadius));
            }

            int baseCenterIndex = vertices.Count;
            vertices.Add(Vector3.zero);
            int tipCenterIndex = vertices.Count;
            vertices.Add(new Vector3(0f, length, 0f));

            for (int i = 0; i < sides; i++)
            {
                int next = (i + 1) % sides;

                int b0 = i * 2;
                int t0 = b0 + 1;
                int b1 = next * 2;
                int t1 = b1 + 1;

                triangles.Add(b0);
                triangles.Add(t0);
                triangles.Add(t1);

                triangles.Add(b0);
                triangles.Add(t1);
                triangles.Add(b1);

                triangles.Add(baseCenterIndex);
                triangles.Add(b1);
                triangles.Add(b0);

                triangles.Add(tipCenterIndex);
                triangles.Add(t0);
                triangles.Add(t1);
            }

            Mesh mesh = new Mesh();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        private static IReadOnlyList<Vector3> GetSpikeDirections(int count)
        {
            Vector3[] dirs = new Vector3[count];
            float offset = 2f / count;
            float increment = Mathf.PI * (3f - Mathf.Sqrt(5f));

            for (int i = 0; i < count; i++)
            {
                float y = ((i * offset) - 1f) + (offset * 0.5f);
                float radius = Mathf.Sqrt(1f - (y * y));
                float phi = i * increment;

                dirs[i] = new Vector3(Mathf.Cos(phi) * radius, y, Mathf.Sin(phi) * radius);
            }

            return dirs;
        }

        private static void ApplyMaterial(GameObject target, Material material)
        {
            MeshRenderer renderer = target.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        private static void RemoveCollider(GameObject target)
        {
            Collider col = target.GetComponent<Collider>();
            if (col != null)
            {
                DestroySafe(col);
            }
        }

        private static void DestroySafe(Object target)
        {
            if (target == null)
            {
                return;
            }

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(target);
                return;
            }
#endif
            Destroy(target);
        }

        private void TryAssignDefaultMaterial()
        {
            if (bodyMaterial != null)
            {
                return;
            }

#if UNITY_EDITOR
            bodyMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/_Blocky_Holes/Materials/Objects/Deadly_Object.mat");
            if (spikeMaterial == null)
            {
                spikeMaterial = bodyMaterial;
            }
#endif
        }
    }
}
