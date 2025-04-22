using MarTools;
using UnityEngine;

[RequireComponent(typeof(Material))]
[ExecuteAlways]
public class SetGradientAsTexture : MonoBehaviour
{
    public string parameterID = "_Gradient";
    public int materialIndex = 0;
    public Gradient gradient;
    public int resolution = 32;

    public bool sharedMaterialDuringRuntime = true;

    private void Start()
    {
        UpdateMaterial();
    }

    private void OnValidate()
    {
        UpdateMaterial();
    }
    private void Update()
    {
        if(!Application.isPlaying)
        {
            UpdateMaterial();
        }
    }

    private void UpdateMaterial()
    {
        var renderer = GetComponent<Renderer>();
        if(Application.isPlaying && !sharedMaterialDuringRuntime)
        {
            renderer.materials[materialIndex].SetTexture(parameterID, Utilities.GenerateGradientTexture(gradient, resolution, 1));
        }
        else
        {
            renderer.sharedMaterials[materialIndex].SetTexture(parameterID, Utilities.GenerateGradientTexture(gradient, resolution, 1));
        }
    }
}
