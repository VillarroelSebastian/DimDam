using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace GumRemix
{
    /// <summary>Menu: Gum Remix / Create Scene. Writes Assets/GumRemix/0_0_Scenes/GumRemix.unity (one bootstrap object, nothing else).</summary>
    public static class GumRemixSceneBuilder
    {
        public const string ScenePath = "Assets/GumRemix/0_0_Scenes/GumRemix.unity";
        public const string PrefabPath = "Assets/GumRemix/0_2_Prefabs/GumRemixBootstrap.prefab";

        [MenuItem("Gum Remix/Create Bootstrap Prefab")]
        public static void CreatePrefab()
        {
            var go = new GameObject("GumRemixBootstrap"); go.AddComponent<GumRemixBootstrap>();
            PrefabUtility.SaveAsPrefabAsset(go, PrefabPath); Object.DestroyImmediate(go);
            AssetDatabase.Refresh();
        }

        [MenuItem("Gum Remix/Create Scene")]
        public static void Create()
        {
            CreatePrefab();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("GumRemixBootstrap");
            go.AddComponent<GumRemixBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
            Debug.Log("Gum Remix scene created: " + ScenePath);
        }

        [MenuItem("Gum Remix/Open Scene")]
        public static void Open()
        {
            if (!System.IO.File.Exists(ScenePath)) Create();
            EditorSceneManager.OpenScene(ScenePath);
        }
    }
}
