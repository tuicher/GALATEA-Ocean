using System.Collections.Generic;
using UnityEngine;

public class GradientSimulation : MonoBehaviour
{
    public ComputeShader _gradientShader;
    public SimplexNoiseParams _simplexNoiseParams;
    public int _resolution = 512;
    public RenderTexture _gradientTexture;
    public Transform _container;
    List<ComputeBuffer> _buffersToRelease;
    public Vector2 _minMax = new Vector2(0, 1);
    public Vector4 _modelingParams = new Vector4(0.5f, 0.5f, 0.5f, 0.5f);

    public void Awake()
    {
        UpdateGradient();
    }
    
    public void UpdateGradient()
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();

        CreateTexture(ref _gradientTexture, _resolution, "GradientTexture");

        if(_gradientShader == null)
            return;

        
        _buffersToRelease = new List<ComputeBuffer>();

        var rnd = new System.Random(_simplexNoiseParams._seed);
        var offsets = new Vector4[_simplexNoiseParams._numLayers];
        for (int i = 0; i < _simplexNoiseParams._numLayers; i++)
        {
            var pt = new Vector4(
               (float) rnd.NextDouble(),
               (float) rnd.NextDouble(), 
               (float) rnd.NextDouble(),
               (float) rnd.NextDouble()
               );
            offsets[i] = (pt * 2 - Vector4.one) * 1000 + (Vector4) _container.position;
        }
        CreateBuffer(offsets, sizeof(float) * 4, "offsets");
        
        var settings = (SimplexNoiseParams.Data) _simplexNoiseParams.GetDataArray().GetValue(0);
        settings.offset += FindAnyObjectByType<CloudSimulation>()._heightOffset;
        CreateBuffer(new SimplexNoiseParams.Data[]{settings}, _simplexNoiseParams.Stride, "noiseSettings");
        
        _gradientShader.SetInt("resolution", _resolution);
        _gradientShader.SetVector("minMax", _minMax);
        _gradientShader.SetVector("params", _modelingParams);
        var buffer = CreateBuffer(new int[] {int.MaxValue, 0}, sizeof(int), "minMaxBuffer");
        _gradientShader.SetBuffer(0, "minMaxBuffer", buffer);
        _gradientShader.SetTexture(0, "Result", _gradientTexture);

        int threadGroupSize = 16;
        int numThreadGroups = Mathf.CeilToInt(_resolution / (float) threadGroupSize);
        _gradientShader.Dispatch(0, numThreadGroups, numThreadGroups, 1);

        for (int i = 0; i < _buffersToRelease.Count; i++)
            _buffersToRelease[i].Release();

        //Debug.Log("Gradient simulation time: " + timer.ElapsedMilliseconds + " ms");
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
                dimension = UnityEngine.Rendering.TextureDimension.Tex2D,
                volumeDepth = resolution,
                enableRandomWrite = true,
                graphicsFormat = format,
                name = name
            };
            
            texture.Create();
        }

        //texture.wrapMode = TextureWrapMode.Repeat;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        //texture.filterMode = FilterMode.Trilinear;
    }

    ComputeBuffer CreateBuffer(System.Array data, int stride, string bufferName, int kernel = 0)
    {
        var buffer = new ComputeBuffer(data.Length, stride, ComputeBufferType.Raw);
        _buffersToRelease.Add(buffer);
        buffer.SetData(data);
        _gradientShader.SetBuffer(kernel, bufferName, buffer);
        return buffer;
    }
}
