using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SliderInputBehavior : Selectable, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent<float> OnUpdateValue;

    [SerializeField] Image fillImage;
    [SerializeField] RectTransform clickArea;

    public int currentValue { get; private set; } = 0;
    public int maxValue = 10;

    public float normalizedValue => (float)currentValue / maxValue;

    public void SetValue(int value)
    {
        int newValue = Mathf.Clamp(value, 0, maxValue);
        int previousValue = currentValue;

        currentValue = newValue;
        if(newValue != previousValue)
        {
            OnUpdateValue.Invoke(normalizedValue);
        }

        UpdateVisual();
    }
    public void SetValue(float progress)
    {
        SetValue(Mathf.RoundToInt(progress * maxValue));
    }

    private void UpdateVisual()
    {
        fillImage.fillAmount = normalizedValue;
    }

    public override void OnMove(AxisEventData eventData)
    {
        // Inspect the move direction
        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                SetValue(currentValue - 1);
                eventData.Use(); // Prevent propagation
                break;
            case MoveDirection.Right:
                SetValue(currentValue + 1);
                eventData.Use(); // Prevent propagation
                break;
            default:
                // Let base handle Up/Down or others
                base.OnMove(eventData);
                break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SetValueFromClick(eventData);
    }

    private void SetValueFromClick(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            clickArea, eventData.pressPosition, eventData.pressEventCamera, out localPoint))
        {
            // Convert localPoint to normalized (0–1) space
            Rect rect = clickArea.rect;
            float normX = Mathf.InverseLerp(rect.xMin, rect.xMax, localPoint.x);
            float normY = Mathf.InverseLerp(rect.yMin, rect.yMax, localPoint.y);

            Vector2 normalized = new Vector2(normX, normY);

            float progress = normalized.x;

            SetValue(progress);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SliderInputBehavior), true)]
[CanEditMultipleObjects]
public class SliderInputBehaviorEditor : Editor 
{
}
#endif