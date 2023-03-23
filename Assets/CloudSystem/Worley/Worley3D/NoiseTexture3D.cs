using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[ExecuteAlways]
public class NoiseTexture3D : MonoBehaviour
{
    [Range(0.0f, 1.0f)] public float _sliceDepth;
    [SerializeField] private ComputeShader _computeShader;
    private Material _material;
    private MeshRenderer _meshRenderer;
    private RenderTexture noiseTexture;

    [SerializeField] private int _textureResolution = 128;
    [SerializeField] private int _cellResolution = 32;
    [SerializeField] private int _axisCellCount = 4;
    [SerializeField] private int _seed = 0;
    [SerializeField] private float _gridSize = 10.0f;

    private ComputeBuffer _computeBuffer;
    private const int _threadGroupSize = 8;

    private void OnEnable()
    {
        _meshRenderer = gameObject.GetComponent<MeshRenderer>();
        _material = _meshRenderer.material;
    }

    private void OnValidate() {
        _material.SetFloat("_SliceDepth", _sliceDepth);
    }

    private void Update()
    {
        GenerateWorleyTexture();
    }

    private void GenerateWorleyTexture()
    {
        CreateRenderTexture(ref noiseTexture, _textureResolution, "_NoiseMap", TextureDimension.Tex3D);

        Random.InitState(_seed);

        _computeShader.SetInt("_Resolution", _textureResolution);
        _computeShader.SetInt("_CellResolution", _cellResolution);
        _computeShader.SetInt("_AxisCellCount", _axisCellCount);
        _computeShader.SetFloat("_GridSize", _gridSize);
        var _feature = Create3DFeaturePointsBuffer();
        _computeShader.SetBuffer(0, "_FeaturePoints", _feature);
        _computeShader.SetTexture(0, "_Result", noiseTexture);

        int threadsPerGroup = Mathf.CeilToInt(_textureResolution / (float)_threadGroupSize);
        _computeShader.Dispatch(0, threadsPerGroup, threadsPerGroup, threadsPerGroup);

        _meshRenderer.sharedMaterial.SetTexture("_BaseMap", noiseTexture);

        _computeBuffer.Release();
        _computeBuffer = null;
    }


    private void CreateRenderTexture(ref RenderTexture renderTexture, int resolution, string name, TextureDimension textureDimension)
    {
        if (renderTexture == null || !renderTexture.IsCreated() || renderTexture.width != resolution || renderTexture.dimension != textureDimension)
        {
            if (renderTexture != null)
            {
                renderTexture.Release();
            }

            renderTexture = new RenderTexture(resolution, resolution, 0)
            {
                enableRandomWrite = true,
                dimension = textureDimension,
                volumeDepth = textureDimension == TextureDimension.Tex3D ? resolution : 0,
                name = name,
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };

            renderTexture.Create();
        }
    }
    private ComputeBuffer Create3DFeaturePointsBuffer()
    {
        // Create one feature point per cell.
        int count = _axisCellCount * _axisCellCount;
        float3[] points = new float3[count];
        for (int i = 0; i < count; i++)
        {
            points[i] = new float3(Random.value, Random.value, Random.value);
        }

        ComputeBuffer computeBuffer = new ComputeBuffer(points.Length, sizeof(float) * 3, ComputeBufferType.Structured);
        computeBuffer.SetData(points);
        
        _computeBuffer = computeBuffer;

        return computeBuffer;
    }
}