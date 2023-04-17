using UnityEngine;

[CreateAssetMenu]
public class WorleyNoiseParams : ScriptableObject
{
    public int seed;

    [Range(1, 50)] public int numDivisionsA = 5;
    [Range(1, 50)] public int numDivisionsB = 10;
    [Range(1, 50)] public int numDivisionsC = 20;

    public float persistence = 0.5f;
    public int tile = 1;
    public bool invert = true;
}
