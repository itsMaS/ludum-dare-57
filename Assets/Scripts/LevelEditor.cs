#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using MarKit;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    public static LevelTemplate debugLevel;

    [MenuItem("Tools/Level Editor")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorWindow>("Level Editor");
    }

    private void OnGUI()
    {
        var prefab = GetCurrentlyEditedPrefabRoot();

        foreach (var item in GetAllPrefabsInFolder("Assets/Prefabs/LevelTemplates"))
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label($"{item.gameObject.name}");
            
            EditorGUILayout.ObjectField(item.gameObject, typeof(GameObject));
            
            if(GUILayout.Button("Load"))
            {
                LoadLevel(item.GetComponent<LevelTemplate>());
            }



            GUILayout.EndHorizontal();
        }
    }

    private void LoadLevel(LevelTemplate template)
    {
        debugLevel = template;
        EditorApplication.EnterPlaymode();
    }

    public static GameObject GetCurrentlyEditedPrefabRoot()
    {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            return stage.prefabContentsRoot;
        }

        return null;
    }

    /// <summary>
    /// Returns all prefab GameObjects found in the specified folder.
    /// </summary>
    /// <param name="folderPath">Relative path in Assets/, e.g., "Assets/Levels"</param>
    /// <param name="includeSubfolders">Include subfolders if true</param>
    public static List<GameObject> GetAllPrefabsInFolder(string folderPath, bool includeSubfolders = true)
    {
        List<GameObject> prefabs = new List<GameObject>();

        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null)
                prefabs.Add(prefab);
        }

        return prefabs;
    }
}
#endif
