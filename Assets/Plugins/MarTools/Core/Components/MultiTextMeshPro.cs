using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiTextMeshPro : MonoBehaviour
{
    public List<TMP_Text> Texts = new List<TMP_Text>();

    public string text;

    public void SetText(string text)
    {
        foreach (var item in Texts)
        {
            item.SetText(text);
        }
    }

    private void OnValidate()
    {
        SetText(text);
    }
}
