using MarTools;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.U2D;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MarKit
{
    [RequireComponent(typeof(HealthBehavior))]
    public class RockBehavior : MarKitBehavior, IGeneratable
    {
        public float hpPerUnitOfArea = 2;
        public Vector2 sizeBounds = new Vector2(3, 3);

        SpriteShapeController controller;
        HealthBehavior entity;

        [SerializeField] float areaSize = 0;

        private void Awake()
        {
            entity = GetComponent<HealthBehavior>();

            entity.OnReceivedDamage.AddListener(Hit);
        }

        protected void Start()
        {
            controller = GetComponent<SpriteShapeController>();
        }

        internal static void GenerateAll()
        {
            FindObjectsOfType<RockBehavior>().ToList().ForEach(x => x.GenerateRock());
        }

        public void GenerateRock()
        {
            if(transform.localScale.x != 1)
            {
                sizeBounds *= transform.localScale.x;
                transform.localScale = Vector3.one;
            }


            controller = GetComponent<SpriteShapeController>();

            GetComponent<Collider2D>().enabled = false;
            GenerateSpriteShape(GetGeneratedPoints());

            entity.maxHealth = Mathf.CeilToInt(areaSize * hpPerUnitOfArea);
            entity.RestoreHealth();
        }

        internal void ResetShape()
        {
            controller = GetComponent<SpriteShapeController>();

            var spline = controller.spline;
            spline.Clear();

            spline.InsertPointAt(0, new Vector3(-0.1f,-0.1f,0));
            spline.InsertPointAt(1, new Vector3(-0.1f,0.1f,0));
            spline.InsertPointAt(2, new Vector3(0.1f,0.1f,0));
            spline.InsertPointAt(3, new Vector3(0.1f,-0.1f,0));

            RefreshShape();
        }

        private void RefreshShape()
        {
            controller.BakeMesh();
            controller.BakeCollider();
            controller.RefreshSpriteShape();

            var bounds = controller.spriteShapeRenderer.localBounds;
            bounds.extents += Vector3.one * 10;
            controller.spriteShapeRenderer.localBounds = bounds;

            controller.UpdateSpriteShapeParameters();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            EditorUtility.SetDirty(controller);
            EditorUtility.SetDirty(transform);
            EditorUtility.SetDirty(gameObject);
#endif
        }

        private void GenerateSpriteShape(List<Vector2> Points)
        {
            var spline = controller.spline;
            spline.Clear();

            for (int i = 0; i < Points.Count; i++)
            {
                spline.InsertPointAt(i, Points[i]);
                spline.SetTangentMode(i, ShapeTangentMode.Linear);
            }

            GetComponent<Collider2D>().enabled = true;

            areaSize = CalculatePolygonArea(Points);

            RefreshShape();
        }

        private List<Vector2> GetGeneratedPoints()
        {
            List<Vector2> RockPositions = new List<Vector2>();
            
            int points = 15;
            float size = sizeBounds.PickRandom();
            
            for (int i = 0; i < points; i++)
            {
                float t = (float)i / points;

                Vector3 castDirection = new Vector3(Mathf.Cos(Mathf.PI * 2 * t), Mathf.Sin(Mathf.PI * 2 * t));
                RaycastHit2D hit = Physics2D.Raycast(transform.position, castDirection, size);
                if (hit)
                {
                    RockPositions.Add((Vector3)hit.point - transform.position - castDirection.normalized*1f);
                }
                else
                {
                    RockPositions.Add(castDirection * size - castDirection.normalized * Random.value.Remap01(0.8f, 1.2f));
                }
            }

            return RockPositions;
        }

        public void Hit()
        {
            controller.spriteShapeRenderer.material.SetFloat("_CrackProgress", 1-entity.normalizedHealth);
        }

        public static float CalculatePolygonArea(List<Vector2> points)
        {
            int count = points.Count;
            if (count < 3)
            {
                Debug.LogWarning("Need at least 3 points to form a polygon.");
                return 0f;
            }

            float area = 0f;

            for (int i = 0; i < count; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % count]; // wrap around to the first point
                area += (current.x * next.y) - (next.x * current.y);
            }

            return Mathf.Abs(area) * 0.5f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, sizeBounds.x);
            Gizmos.DrawWireSphere(transform.position, sizeBounds.y);
        }

        public void Generate(params object[] argumants)
        {
            GenerateRock();
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RockBehavior))]
    [CanEditMultipleObjects]
    public class RockBehaviorEditor : MarToolsEditor<RockBehavior>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                script.GenerateRock();
            }


            if(GUILayout.Button("Generate All"))
            {
                RockBehavior.GenerateAll();
            }

            if(GUILayout.Button("Reset"))
            {
                script.ResetShape();
            }
        }

        private void OnSceneGUI()
        {
        }
    }
#endif
}
