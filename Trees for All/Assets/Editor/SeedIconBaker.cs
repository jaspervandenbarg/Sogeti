using System.Collections.Generic;
using System.IO;
using Sogeti.Planting;
using UnityEditor;
using UnityEngine;

namespace Sogeti.Editor
{
    /// <summary>
    /// Renders one menu icon per seed from its last growth stage.
    /// Stages 1 and 2 share art across seed types on purpose, so only the grown
    /// tree tells the four species apart. A baker also covers a fifth seed with
    /// one menu click, which hand drawn art does not.
    /// </summary>
    public static class SeedIconBaker
    {
        private const string OutputFolder = "Assets/Textures/UI/SeedIcons";
        private const int IconSize = 256;

        private static readonly Color Background = new Color(0.16f, 0.19f, 0.16f, 1f);

        [MenuItem("Trees for All/Bake Seed Icons")]
        public static void BakeAll()
        {
            List<SeedDefinition> seeds = FindSeeds();
            if (seeds.Count == 0)
            {
                Debug.LogWarning("Seed icon baker: no SeedDefinition asset found.");
                return;
            }

            Directory.CreateDirectory(OutputFolder);

            int baked = 0;
            try
            {
                for (int i = 0; i < seeds.Count; i++)
                {
                    SeedDefinition seed = seeds[i];
                    EditorUtility.DisplayProgressBar("Bake Seed Icons", seed.name, (float)i / seeds.Count);

                    if (BakeOne(seed))
                    {
                        baked++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"Seed icon baker: {baked} of {seeds.Count} icons written to {OutputFolder}.");
        }

        private static bool BakeOne(SeedDefinition seed)
        {
            GameObject prefab = LastStageVisual(seed);
            if (prefab == null)
            {
                Debug.LogWarning($"Seed icon baker: {seed.name} has no visual on its last stage.", seed);
                return false;
            }

            Texture2D texture = Render(prefab);
            if (texture == null)
            {
                Debug.LogWarning($"Seed icon baker: {seed.name} rendered nothing.", seed);
                return false;
            }

            string path = $"{OutputFolder}/SeedIcon{seed.name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            ApplySpriteSettings(path);
            AssignIcon(seed, AssetDatabase.LoadAssetAtPath<Sprite>(path));
            return true;
        }

        private static GameObject LastStageVisual(SeedDefinition seed)
        {
            for (int i = seed.StageCount - 1; i >= 0; i--)
            {
                GrowthStage stage = seed.GetStage(i);
                if (stage != null && stage.VisualPrefab != null)
                {
                    return stage.VisualPrefab;
                }
            }

            return null;
        }

        private static Texture2D Render(GameObject prefab)
        {
            PreviewRenderUtility preview = new PreviewRenderUtility();
            GameObject instance = null;

            try
            {
                preview.BeginStaticPreview(new Rect(0f, 0f, IconSize, IconSize));

                instance = Object.Instantiate(prefab);
                instance.hideFlags = HideFlags.HideAndDontSave;
                instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                preview.AddSingleGO(instance);

                Bounds bounds = MeshBounds(instance);
                FrameCamera(preview.camera, bounds);
                SetUpLights(preview);

                preview.Render();
                return preview.EndStaticPreview();
            }
            finally
            {
                if (instance != null)
                {
                    Object.DestroyImmediate(instance);
                }

                preview.Cleanup();
            }
        }

        // Qualified, because Sogeti.Camera is a namespace in this project.
        private static void FrameCamera(UnityEngine.Camera camera, Bounds bounds)
        {
            // A three quarter view separates a pine from an oak better than a flat side view.
            Quaternion rotation = Quaternion.Euler(12f, 140f, 0f);
            float radius = Mathf.Max(bounds.extents.magnitude, 0.1f);

            camera.orthographic = true;
            camera.orthographicSize = radius * 1.05f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            camera.nearClipPlane = 0.01f;
            camera.farClipPlane = radius * 10f;
            camera.transform.rotation = rotation;
            camera.transform.position = bounds.center - rotation * Vector3.forward * (radius * 4f);
        }

        private static void SetUpLights(PreviewRenderUtility preview)
        {
            preview.lights[0].intensity = 1.3f;
            preview.lights[0].transform.rotation = Quaternion.Euler(40f, 120f, 0f);
            preview.lights[1].intensity = 0.5f;
            preview.lights[1].transform.rotation = Quaternion.Euler(20f, -60f, 0f);
        }

        private static Bounds MeshBounds(GameObject instance)
        {
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one);
            }

            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void ApplySpriteSettings(string path)
        {
            if (AssetImporter.GetAtPath(path) is not TextureImporter importer)
            {
                return;
            }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100f;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = IconSize;

            // The bake is opaque, so transparency handling would only cost fill rate.
            importer.alphaIsTransparency = false;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);

            // A tight mesh clips the icon under Preserve Aspect.
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            importer.SetTextureSettings(settings);

            importer.SaveAndReimport();
        }

        private static void AssignIcon(SeedDefinition seed, Sprite sprite)
        {
            if (sprite == null)
            {
                Debug.LogWarning($"Seed icon baker: {seed.name} produced no sprite.", seed);
                return;
            }

            SerializedObject serialized = new SerializedObject(seed);
            SerializedProperty property = serialized.FindProperty("menuIcon");
            if (property == null)
            {
                Debug.LogError($"Seed icon baker: {seed.name} has no menuIcon field.", seed);
                return;
            }

            property.objectReferenceValue = sprite;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(seed);
        }

        private static List<SeedDefinition> FindSeeds()
        {
            List<SeedDefinition> seeds = new List<SeedDefinition>();
            foreach (string guid in AssetDatabase.FindAssets($"t:{nameof(SeedDefinition)}"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                SeedDefinition seed = AssetDatabase.LoadAssetAtPath<SeedDefinition>(path);
                if (seed != null)
                {
                    seeds.Add(seed);
                }
            }

            return seeds;
        }
    }
}
