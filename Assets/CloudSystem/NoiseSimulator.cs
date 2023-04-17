using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;

public class NoiseSimulator : MonoBehaviour
{
    public enum NoiseType { Volume, Detail }
    public enum Channel { R, G, B, A }
    [SerializeField] public NoiseType _activeTextureType;
    [SerializeField] public Channel _activeChannel;
    [SerializeField] public bool _simulate = true;
    [SerializeField] ComputeShader _computeShader;
    [SerializeField] ComputeShader _slicerShader;
    [SerializeField] ComputeShader _copyShader;
    const int ThreadGroupSize = 8;

    public const string VolumeNoise = "VolumeNoise";
    public const string DetailNoise = "DetailNoise";

    public int _volumeResolution = 128;
    public int _detailResolution = 32;

    public WorleyNoiseParams[] _volumeNoiseParams;
    public WorleyNoiseParams[] _detailNoiseParams;

    [SerializeField] public RenderTexture _volumeTexture;
    [SerializeField] public RenderTexture _detailTexture;

    [HideInInspector] List<ComputeBuffer> _buffersToRelease;

    private void Awake()
    {
        _activeTextureType = NoiseType.Volume;
        _simulate = true;
        UpdateNoise();
        _activeTextureType = NoiseType.Detail;
        _simulate = true;
        UpdateNoise();

    }

    public void Load(string name, RenderTexture target)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        name = sceneName + "_" + name;
        Texture3D savedTexture = Resources.Load<Texture3D>(name);
        if (savedTexture != null && savedTexture.width == target.width)
        {
            _copyShader.SetTexture(0, "tex", savedTexture);
            _copyShader.SetTexture(0, "renderTex", target);
            int n = Mathf.CeilToInt(target.width / 8.0f);
            _copyShader.Dispatch(0, n, n, n);

            //Graphics.CopyTexture(savedTexture, target);
           // print("copiado");
        }
    }

    public void Save(string name, RenderTexture target)
    {
        string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        name = sceneName + "_" + name;
        int resolution = target.width;
        Texture2D[] slices = new Texture2D[resolution];

        _slicerShader.SetInt("resolution", resolution);
        _slicerShader.SetTexture(0, "volumeTexture", target);

        for (int i = 0; i < resolution; i++)
        {
            var slice = new RenderTexture(resolution, resolution, 0);
            slice.dimension = TextureDimension.Tex2D;
            slice.enableRandomWrite = true;

            _slicerShader.SetInt("layer", i);
            _slicerShader.SetTexture(0, "slice", slice);
            _slicerShader.Dispatch(0, Mathf.CeilToInt(resolution / 32.0f), Mathf.CeilToInt(resolution / 32.0f), 1);

            Texture2D sliceTexture = new Texture2D(slice.width, slice.height);
            RenderTexture.active = slice;
            sliceTexture.ReadPixels(new Rect(0, 0, slice.width, slice.height), 0, 0);
            sliceTexture.Apply();
            slices[i] = sliceTexture;
        }

        Texture3D texture3D = new Texture3D(resolution, resolution, resolution, TextureFormat.ARGB32, false);
        texture3D.filterMode = FilterMode.Trilinear;
        Color[] pixels = texture3D.GetPixels();

        for (int k = 0; k < resolution; k++)
        {
            Color c = slices[k].GetPixel(0, 0);
            Color[] layerPixels = slices[k].GetPixels();
            for (int i = 0; i < resolution; i++)
                for (int j = 0; j < resolution; j++)
                {
                    pixels[i + resolution * ( j + k * resolution)] = layerPixels[i + j * resolution];
                }
        }
        texture3D.SetPixels(pixels);
        texture3D.Apply();

        UnityEditor.AssetDatabase.CreateAsset(texture3D, "Assets/Resources/" + name + ".asset");
    }

    ComputeBuffer CreateBuffer(System.Array data, int stride, string bufferName, int kernel = 0)
    {
        var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Structured);
        _buffersToRelease.Add(buffer);
        buffer.SetData(data);
        _computeShader.SetBuffer(kernel, bufferName, buffer);
        return buffer;
    }

    public void CreateTexture(ref RenderTexture texture, int resolution, string name)
    {
        var format = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_UNorm;
        if (texture == null || !texture.IsCreated() ||
        texture.width != resolution || texture.height != resolution || texture.volumeDepth != resolution ||
        texture.graphicsFormat != format)
        {
            if (texture != null)
                texture.Release();

            texture = new RenderTexture(resolution, resolution, 0, format)
            {
                dimension = TextureDimension.Tex3D,
                volumeDepth = resolution,
                enableRandomWrite = true,
                name = name
            };
            texture.Create();
            Load(name, texture);
        }

        texture.wrapMode = TextureWrapMode.Repeat;
        texture.filterMode = FilterMode.Bilinear;
        //texture.filterMode = FilterMode.Trilinear;
    }

    public WorleyNoiseParams ActiveParams
    {
        get
        {
            WorleyNoiseParams[] noiseParams = (_activeTextureType == NoiseType.Volume) ? _volumeNoiseParams : _detailNoiseParams;
            return noiseParams[(int)_activeChannel];
        }
    }

    public RenderTexture ActiveTexture
    {
        get
        {
            return (_activeTextureType == NoiseType.Volume) ? _volumeTexture : _detailTexture;
        }
    }

    public Vector4 ActiveChannelMask
    {
        get
        {
            switch (_activeChannel)
            {
                case Channel.R:
                    return new Vector4(1.0f, 0.0f, 0.0f, 0.0f);
                case Channel.G:
                    return new Vector4(0.0f, 1.0f, 0.0f, 0.0f);
                case Channel.B:
                    return new Vector4(0.0f, 0.0f, 1.0f, 0.0f);
                case Channel.A:
                    return new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            }
            return Vector4.zero;
        }
    }


    public void UpdateNoise()
    {
        CreateTexture(ref _volumeTexture, _volumeResolution, VolumeNoise);
        CreateTexture(ref _detailTexture, _detailResolution, DetailNoise);

        if (_simulate && _computeShader)
        {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            _simulate = false;
            WorleyNoiseParams activeParams = ActiveParams;
            if (activeParams == null)
                return;

            _buffersToRelease = new List<ComputeBuffer>();

            int textureResolution = ActiveTexture.width;

            //Setting values
            _computeShader.SetInt("resolution", textureResolution);
            _computeShader.SetFloat("persistence", activeParams.persistence);
            _computeShader.SetVector("channelMask", ActiveChannelMask);

            //Setting buffers
            _computeShader.SetTexture(0, "Result", ActiveTexture);
            var minMaxBuffer = CreateBuffer(new int[] { int.MaxValue, 0 }, sizeof(int), "minMax", 0);
            UpdateWorley(ActiveParams);
            _computeShader.SetTexture(0, "Result", ActiveTexture);

            // Dispatch noise gen kernel
            int numThreadGroups = Mathf.CeilToInt(textureResolution / (float)ThreadGroupSize);
            _computeShader.Dispatch(0, numThreadGroups, numThreadGroups, numThreadGroups);

            // Set normalization kernel data:
            _computeShader.SetBuffer(1, "minMax", minMaxBuffer);
            _computeShader.SetTexture(1, "Result", ActiveTexture);
            // Dispatch normalization kernel
            _computeShader.Dispatch(1, numThreadGroups, numThreadGroups, numThreadGroups);

            var minMax = new int[2];
            minMaxBuffer.GetData(minMax);

            Debug.Log($"Noise Generation: {timer.ElapsedMilliseconds}ms");

            foreach (var buffer in _buffersToRelease)
                buffer.Release();
        }
    }

    public void SaveTextures()
    {
        Save( VolumeNoise, _volumeTexture);
        Save( DetailNoise, _detailTexture);
    }

    void UpdateWorley(WorleyNoiseParams settings)
    {
        var prng = new System.Random(settings.seed);
        CreateWorleyPointsBuffer(prng, settings.numDivisionsA, "pointsA");
        CreateWorleyPointsBuffer(prng, settings.numDivisionsB, "pointsB");
        CreateWorleyPointsBuffer(prng, settings.numDivisionsC, "pointsC");

        _computeShader.SetInt("numCellsA", settings.numDivisionsA);
        _computeShader.SetInt("numCellsB", settings.numDivisionsB);
        _computeShader.SetInt("numCellsC", settings.numDivisionsC);
        _computeShader.SetBool("invertNoise", settings.invert);
        _computeShader.SetInt("tile", settings.tile);

    }

    void CreateWorleyPointsBuffer(System.Random prng, int numCellsPerAxis, string bufferName)
    {
        var points = new Vector3[numCellsPerAxis * numCellsPerAxis * numCellsPerAxis];
        float cellSize = 1f / numCellsPerAxis;

        for (int x = 0; x < numCellsPerAxis; x++)
            for (int y = 0; y < numCellsPerAxis; y++)
                for (int z = 0; z < numCellsPerAxis; z++)
                {
                    float randomX = (float)prng.NextDouble();
                    float randomY = (float)prng.NextDouble();
                    float randomZ = (float)prng.NextDouble();
                    Vector3 randomOffset = new Vector3(randomX, randomY, randomZ) * cellSize;
                    Vector3 cellCorner = new Vector3(x, y, z) * cellSize;

                    int index = x + numCellsPerAxis * (y + z * numCellsPerAxis);
                    points[index] = cellCorner + randomOffset;
                }

        CreateBuffer(points, sizeof(float) * 3, bufferName);
    }
}
