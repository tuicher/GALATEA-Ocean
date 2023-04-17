using UnityEngine;

[CreateAssetMenu]
public class SimplexNoiseParams : ScriptableObject
{
    public struct Data
    {
        public int seed;
        public int numLayers;
        public float scale;
        public float lacunarity;
        public float persistence;
        public Vector2 offset;
    }

    public int _seed;
    [Range(1, 6)] public int _numLayers = 1;
    public float scale = 1.0f;
    public float _persistence = 0.5f;
    public float _lacunarity = 2.0f;
    public Vector2 offset;
    public event System.Action OnValueChanged;

    void OnValidate()
    {
        if(OnValueChanged != null)
            OnValueChanged();
    }

    public Data GetData()
    {
        Data data = new Data()
        {
            seed = _seed,
            numLayers = _numLayers,
            scale = scale,
            persistence = _persistence,
            lacunarity = _lacunarity,
            offset = offset,
        };
        return data;
    }

    public System.Array GetDataArray()
    {
        Data[] dataArray = new Data[1];
        dataArray[0] = GetData();
        return dataArray;
    }

    public int Stride
    {
        get 
        {
            return sizeof(float) * 7;
        }
    }
}
