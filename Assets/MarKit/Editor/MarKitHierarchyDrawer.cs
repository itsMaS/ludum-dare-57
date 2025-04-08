using UnityEditor;
using UnityEngine;

namespace MarKit
{
    [InitializeOnLoad]
    public static class CustomHierarchyDrawer
    {
        static CustomHierarchyDrawer()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            if(obj.name.Contains("//"))
            {
                EditorGUI.DrawRect(selectionRect, new Color(0, 0, 0, 1));

                GUIStyle skin = new GUIStyle(GUI.skin.label);
                skin.normal.textColor = Color.white;
                skin.alignment = TextAnchor.MiddleCenter;

                GUI.Label(selectionRect, $"=== {obj.name.Remove(0, 2)} ===", skin); // no label
            }

            // Get first script (MonoBehaviour) on the GameObject
            MonoBehaviour firstScript = null;
            foreach (var comp in obj.GetComponents<MonoBehaviour>())
            {
                if (comp != null)
                {
                    firstScript = comp;
                    break;
                }
            }

            if (firstScript == null) return;

            // Get Unity's default icon for the script
            GUIContent scriptIconContent = EditorGUIUtility.ObjectContent(firstScript, firstScript.GetType());
            Texture2D icon = scriptIconContent.image as Texture2D;

            if (icon == null) return;

            // Draw icon on the left side of the hierarchy item

            Rect iconRect = new Rect(selectionRect.x - 2, selectionRect.y + 1, 16, 16);

            EditorGUI.DrawRect(iconRect, new Color(0, 0, 0, 1));
            GUI.DrawTexture(iconRect, icon);
        }
    }
}
