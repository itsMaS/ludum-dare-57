using TMPro;
using UnityEngine;

[ExecuteAlways]
public class OffscreenIndicator : MonoBehaviour
{
    [SerializeField] private Transform trackedObject;
    [SerializeField] private Camera mainCamera => Camera.main;
    [SerializeField] private float edgeBuffer = 0.5f; // How far inside the screen edge to place the indicator

    [SerializeField] TextMeshPro distanceText;

    public Vector3 offset = Vector3.zero;

    public bool lockX = false;

    private void Update()
    {
        if (trackedObject == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(trackedObject.position);

        // Check if the object is offscreen (or behind the camera)
        bool isOffscreen = viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1;

        if (isOffscreen)
        {
            // Clamp to screen edge while preserving direction
            Vector3 clampedViewportPos = viewportPos;

            // If behind the camera, flip the direction
            if (viewportPos.z < 0)
            {
                clampedViewportPos.x = 1f - clampedViewportPos.x;
                clampedViewportPos.y = 1f - clampedViewportPos.y;
                clampedViewportPos.z = Mathf.Abs(clampedViewportPos.z);
            }

            clampedViewportPos.x = Mathf.Clamp01(clampedViewportPos.x);
            clampedViewportPos.y = Mathf.Clamp01(clampedViewportPos.y);
            clampedViewportPos.z = Mathf.Max(1f, clampedViewportPos.z); // keep it in front of the camera

            // Convert back to world position
            Vector3 worldEdgePosition = mainCamera.ViewportToWorldPoint(clampedViewportPos);

            // Nudge the position inward from the edge

            Vector3 toEdgePosition = worldEdgePosition - trackedObject.position;

            Vector3 target = worldEdgePosition + toEdgePosition.normalized * edgeBuffer;

            if (lockX) target.x = Camera.main.transform.position.x;

            target += offset;

            transform.position = target;

            if(distanceText)
            {
                distanceText.SetText($"{Mathf.RoundToInt(toEdgePosition.magnitude)}M");
            }
        }
        else
        {
            // Move it offscreen or deactivate
            transform.position = Vector3.one * 9999f;
        }
    }

}
