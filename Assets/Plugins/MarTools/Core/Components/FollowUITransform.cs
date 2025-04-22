using UnityEngine;

[ExecuteAlways]
public class FollowUITransform : MonoBehaviour
{
    [SerializeField] RectTransform follow;
    [SerializeField] Camera uiCamera => canvas.worldCamera;
    [SerializeField] Vector3 offset;  // Optional offset from followed position

    Canvas canvas;


    private void Update()
    {
        if (!follow) return;
        if(!canvas) canvas = follow.GetComponentInParent<Canvas>();


        Vector3 worldPos;

        // Convert RectTransform position to screen space
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, follow.position);

        float distanceFromCamera = canvas.planeDistance + canvas.worldCamera.transform.position.z;

        // Convert screen position to world space
        Ray ray = uiCamera ? uiCamera.ScreenPointToRay(screenPos) : Camera.main.ScreenPointToRay(screenPos);
        Plane plane = new Plane(Vector3.forward, -distanceFromCamera); // Adjust the plane as needed for your world

        if (plane.Raycast(ray, out float distance))
        {
            worldPos = ray.GetPoint(distance);
            transform.position = worldPos + offset;
        }
    }
}
