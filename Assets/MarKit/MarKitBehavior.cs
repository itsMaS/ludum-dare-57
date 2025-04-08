using UnityEngine;
using System.Reflection;
using MarTools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    public class MarKitBehavior : MonoBehaviour, IMarkitEventCaller
    {
        private void Awake()
        {
            Initialize();
        }

        void Initialize()
        {
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(MarKitBehavior), true)]
    public class MarKitBehaviorEditor : Editor
    {
    }
#endif
}
