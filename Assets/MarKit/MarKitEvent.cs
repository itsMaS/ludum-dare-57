using System.Collections.Generic;
using UnityEngine;
using MarTools;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    public interface IMarkitEventCaller
    {
        public GameObject gameObject { get; }
        public Transform transform { get; }
        public MonoBehaviour behavior => this as MonoBehaviour;
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(IMarKitAction), true)]
    [System.Serializable]
    public class IMarkitActionDrawer : PolymorphicDrawer<IMarKitAction> { }
#endif

    public class MarKitEventBase
    {
    }


    [System.Serializable]
    public class MarKitEvent : MarKitEventBase
    {

        [SerializeReference] public List<IMarKitAction> Actions = new List<IMarKitAction>();
        
        private UnityAction action;
        public void Invoke(IMarkitEventCaller parent)
        {
            foreach (var action in Actions)
            {
                action.Invoke(parent, this);
            }
            action?.Invoke();
        }

        public void AddListener(UnityAction action)
        {
            this.action += action;
        }

        public void RemoveListener(UnityAction action) 
        {
            this.action -= action;
        }
    }

    public class MarKitEvent<Args> : MarKitEventBase
    {
                private UnityAction action;
        public void Invoke(IMarkitEventCaller parent,  Args args) 
        {

        }
        public void AddListener(UnityAction action)
        {
            this.action += action;
        }

        public void RemoveListener(UnityAction action)
        {
            this.action -= action;
        }
    }



#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(MarKitEvent))]
    public class MarKitEventDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            // Draw a box around the event
            Rect boxRect = new Rect(position.x, position.y, position.width, GetPropertyHeight(property, label));
            GUI.Box(boxRect, GUIContent.none, EditorStyles.helpBox);

            // Horizontal group for label and Invoke button
            Rect headerRect = new Rect(position.x + 4, position.y + 5, position.width - 8, lineHeight);
            float buttonWidth = 60f;
            float labelWidth = headerRect.width - buttonWidth - 4;

            Rect labelRect = new Rect(headerRect.x + 20, headerRect.y, labelWidth-20, lineHeight);
            Rect buttonRect = new Rect(labelRect.xMax + 4, headerRect.y, buttonWidth, lineHeight);

            EditorGUI.LabelField(labelRect, label);

            if (GUI.Button(buttonRect, "Invoke"))
            {
                object target = property.serializedObject.targetObject;
                var field = target.GetType().GetField(property.name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    MarKitEvent evt = field.GetValue(target) as MarKitEvent;
                    evt?.Invoke(property.objectReferenceValue as IMarkitEventCaller);
                }
            }

            // Draw property field inside box, below header
            Rect contentRect = new Rect(
                position.x + 20,
                headerRect.yMax + spacing -20,
                position.width - 8,
                EditorGUI.GetPropertyHeight(property, true) - lineHeight - spacing
            );

            EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float totalHeight = EditorGUIUtility.singleLineHeight + spacing;
            totalHeight += EditorGUI.GetPropertyHeight(property, true);
            return totalHeight -10; // some padding for box
        }
    }
#endif
}
