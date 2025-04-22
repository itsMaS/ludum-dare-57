using UnityEngine;
using UnityEngine.Events;

public class GenerationBehavior : MonoBehaviour
{
    public UnityEvent OnGenerate;

    public int priority = 0;

    public void Generate()
    {
        OnGenerate.Invoke();
    }
}
