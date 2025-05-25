using UnityEngine;
using System.Collections.Generic;
using MarTools;
using JetBrains.Annotations;
using System.Linq;
using UnityEngine.UIElements;
using MarKit;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu]
public class GameSettingsSO : ScriptableObject
{
    [SerializeField]
    [System.Serializable]
    public class LevelBracket
    {
        public int separator = 0;
        public List<LevelTemplate> Levels;
    }

    public List<LevelBracket> LevelBrackets = new List<LevelBracket>();


    public LevelTemplate GetLevel(int level)
    {
        var bracket = LevelBrackets.Find(x => x.separator < level);
        Debug.Log(bracket.separator);

        if(bracket.Levels.Count > 0)
        {
            return bracket.Levels.PickRandom();
        }
        return null;
    }

    private void OnValidate()
    {
        GetLevel(0);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(GameSettingsSO))]
public class GameSettingsSOEditor : MarToolsEditor<GameSettingsSO>
{
    SerializedObject serializedObj;

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        //base.OnInspectorGUI();

        script.LevelBrackets = script.LevelBrackets.OrderBy(x => x.separator).ToList();

        serializedObj = new SerializedObject(script);

        for (int i = 0; i < script.LevelBrackets.Count - 1; i++)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();

            var currentBracket = script.LevelBrackets[i];
            var nextBracket = script.LevelBrackets[i + 1];

            int currentSeparator = currentBracket.separator;
            int nextSeparator = nextBracket.separator;


            GUILayout.Box($"Levels [{currentSeparator} - {(i == script.LevelBrackets.Count-2 ? "∞" : nextSeparator)}]");


            if (GUILayout.Button("X", GUILayout.Width(20f)))
            {
                script.LevelBrackets.RemoveAt(i);
                EditorUtility.SetDirty(script);
                break;
            }

            GUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(serializedObj.FindProperty("LevelBrackets").GetArrayElementAtIndex(i));
            GUILayout.EndVertical();

            serializedObj.ApplyModifiedProperties();

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(5));
        }

        if(GUILayout.Button("Add bracket"))
        {
            if(script.LevelBrackets.Count > 0)
            {
                script.LevelBrackets.Add(new GameSettingsSO.LevelBracket() { separator = script.LevelBrackets.Last().separator+1 });
            }
            else
            {
                script.LevelBrackets.Add(new GameSettingsSO.LevelBracket());
            }

            EditorUtility.SetDirty(script);
        }

        if(EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(script);
        }
    }
}
#endif
