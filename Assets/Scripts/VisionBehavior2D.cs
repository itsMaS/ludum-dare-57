using UnityEngine;
using MarTools;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    public interface IVisible2D
    {
        public Transform transform { get; }
        public GameObject gameObject { get; }
    }

    public class VisionBehavior2D : MarKitBehavior
    {
        public UnityEvent<IVisible2D> OnDetectStart;
        public UnityEvent<IVisible2D> OnDetectEnd;

        public float radius = 3;
        public List<IVisible2D> Visible { get; private set; } = new List<IVisible2D>();

        private void FixedUpdate()
        {
            var newVisible = new HashSet<IVisible2D>();

            // Detect new objects in range
            foreach (var collider in Physics2D.OverlapCircleAll(transform.position, radius))
            {
                if (collider.TryGetComponent<IVisible2D>(out var visible))
                {
                    Vector2 toVisible = visible.transform.position - transform.position;

                    Debug.DrawLine(transform.position, visible.transform.position, Color.blue, Time.fixedDeltaTime);

                    bool lineOfSight = true;
                    foreach (var item in Physics2D.RaycastAll(transform.position, toVisible.normalized, radius))
                    {
                        if(item.collider.transform == visible.transform)
                        {
                            Debug.DrawLine(transform.position, visible.transform.position, Color.green, Time.fixedDeltaTime);

                            if (newVisible.Add(visible) && !Visible.Contains(visible))
                            {
                                OnDetectStart.Invoke(visible);
                            }
                            break;
                        }
                        else
                        {
                            if (item.collider.gameObject != gameObject)
                            {
                                Debug.DrawLine(transform.position, item.point, Color.red, Time.fixedDeltaTime);
                                lineOfSight = false;
                                break;
                            }
                        }
                    }
                }
            }

            // Detect objects that are no longer visible
            foreach (var previouslyVisible in Visible.ToArray())
            {
                if (!newVisible.Contains(previouslyVisible))
                {
                    OnDetectEnd.Invoke(previouslyVisible);
                }
            }

            // Update Visible to reflect current visibility
            Visible = newVisible.ToList();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(VisionBehavior2D))]
    public class VisionBehavior2DEditor : MarToolsEditor<VisionBehavior2D>
    {
        private void OnSceneGUI()
        {
            Handles.color = Color.red.SetAlpha(script.Visible.Count == 0 ? 0.05f : 0.1f);
            Handles.DrawSolidDisc(script.transform.position, Vector3.forward, script.radius);
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Label($"Detected Objects:");
            foreach (var item in script.Visible)
            {
                GUILayout.Label($"{item.gameObject.name}");
            }
        }
    }
#endif
}
