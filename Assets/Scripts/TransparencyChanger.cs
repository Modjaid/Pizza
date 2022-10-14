using UnityEngine;

public class TransparencyChanger : MonoBehaviour
{
    private Material oldMaterial;
    new private Renderer renderer;
    private float targetTransparency;
    private float transparencyChangeSpeed = 100000f;

    // This value must be set via script when the component is created.
    [HideInInspector] public Shader onlyShadows;

    private void Awake()
    {
        renderer = GetComponent<Renderer>();
        oldMaterial = renderer.material;
    }

    public void ChangeTransparency(bool transparent)
    {
        if (transparent)
        {
            renderer.materials = new Material[2];
            renderer.material = new Material(oldMaterial);
            renderer.material.shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");
            renderer.materials[1] = new Material(oldMaterial);
            renderer.materials[1].shader = Shader.Find("OnlyShadows");
            //targetTransparency = 0.3f;
            float transparency = 0.3f;
            Color color = renderer.material.color;
            color.a = transparency;
            renderer.material.color = color;
        }
        else
        {
            renderer.materials = new Material[1];
            renderer.material = oldMaterial;
        }
    }
}