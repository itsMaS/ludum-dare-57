using UnityEngine;

public class OpenLinkBehavior : MonoBehaviour
{
    public string link;

    public void Open()
    {
        if (!string.IsNullOrEmpty(link))
        {
            Application.OpenURL(link);
        }
        else
        {
            Debug.LogWarning("No link specified to open.");
        }
    }
}