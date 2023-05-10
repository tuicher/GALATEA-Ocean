using UnityEngine;

[CreateAssetMenu(fileName = "SkyParams", menuName = "Skybox/SkyParams")]
public class SkyParams : ScriptableObject
{
    public Gradient skyColor;
    public float sunIntensity;
    public float scatterIntensity;
    public float starsIntensity;
    public float milkywayaIntensity;
    [HideInInspector]
    public Texture2D skyGradientTexture;
}
