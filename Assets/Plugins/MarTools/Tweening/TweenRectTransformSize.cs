using MarTools;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class TweenRectTransformSize : TweenCore
{
    RectTransform rt;

    public Vector2 start;
    public Vector2 end;

    protected override void Reset()
    {
        base.Reset();

        if (!rt) rt = GetComponent<RectTransform>();
        start = rt.sizeDelta;
        end = rt.sizeDelta;
    }

    public override void SetPose(float t)
    {
        if (!rt) rt = GetComponent<RectTransform>();

        rt.sizeDelta = Vector2.Lerp(start, end, t);
    }
}
