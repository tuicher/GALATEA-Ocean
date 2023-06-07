using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class SkyboxController : MonoBehaviour
{
    public bool isPlaying = true;
    [Range(0f, 24f)] public float _time = 9f;
    public Texture2D _starsTexture;
    public Texture2D _moonTexture;
    public Light _sun;
    public Transform _sunTransform;
    public Light _moon;
    public Transform _moonTransform;
    public Transform _milkuWayTransform;
    public Material _skyboxMaterial;
    public SkyParams[] _skyParams;
    public bool debug;
    private SkyParams _newData;
    private SkyParams _skyData;
    private float _updateGlobalIllumination = 0.0f;

    public Texture2D GenerateSkyGradientTexture(Gradient from, Gradient to, int resolution, float k)
    {
        Texture2D texture = new Texture2D(resolution, 1, TextureFormat.RGBAFloat, false, true);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < resolution; i++)
        {
            Color a = from.Evaluate(i / (float)resolution).linear;
            Color b = to.Evaluate(i / (float)resolution).linear;

            texture.SetPixel(i, 0, Color.Lerp(a, b, k));
        }
        texture.Apply(false, false);

        return texture;
    }

    public SkyParams GetSkyColor(float t)
    {
        int index = (int)(t / 3) % _skyParams.Length;
        float lerpFactor = t % 3 / 3.0f;

        _newData = ScriptableObject.CreateInstance < SkyParams > ();
        SkyParams from, to;
        from = _skyParams[index];
        to = _skyParams[(index + 1) % _skyParams.Length];

        _newData.skyGradientTexture = GenerateSkyGradientTexture(from.skyColor, to.skyColor, 256, lerpFactor);
        _newData.sunIntensity = Mathf.Lerp(from.sunIntensity, to.sunIntensity, lerpFactor);
        _newData.scatterIntensity = Mathf.Lerp(from.scatterIntensity, to.scatterIntensity, lerpFactor);
        _newData.starsIntensity = Mathf.Lerp(from.starsIntensity, to.starsIntensity, lerpFactor);
        _newData.milkywayaIntensity = Mathf.Lerp(from.milkywayaIntensity, to.milkywayaIntensity, lerpFactor);

        return _newData;
    }

    public Vector3 _ecliptic = new Vector3(1.0f, 0.45f, 0);

    private Vector3 EclipticAxis => _ecliptic.normalized;
    public void TransformBodies()
    {
        _sun.transform.rotation = Quaternion.AngleAxis((_time - 6) * 360 / 24, EclipticAxis);
        _moon.transform.rotation = Quaternion.AngleAxis((_time - 18) * 360 / 24, EclipticAxis);
        // Debug.Log(-_sun.transform.forward);
        _sunTransform.eulerAngles = _sun.transform.eulerAngles;
        _moonTransform.eulerAngles = _moon.transform.eulerAngles;

        _skyboxMaterial.SetVector("_SunDirectionWS", _sun.transform.forward);
        _skyboxMaterial.SetVector("_MoonDirectionWS", _moon.transform.forward);
    }

    public void SetShaderProperties()
    {
        _skyboxMaterial.SetTexture("_SkyGradientTex", _skyData.skyGradientTexture);
        _skyboxMaterial.SetFloat("_SunIntensity", _skyData.sunIntensity);
        _skyboxMaterial.SetFloat("_ScatteringIntensity", _skyData.scatterIntensity);
        _skyboxMaterial.SetTexture("_StarTex", _starsTexture);
        _skyboxMaterial.SetFloat("_Star Intensity", _skyData.starsIntensity);
        _skyboxMaterial.SetFloat("_MilkywayIntensity", _skyData.milkywayaIntensity);
        _skyboxMaterial.SetTexture("_MoonTex", _moonTexture);

        _skyboxMaterial.SetMatrix("_MoonWorld2Obj", _moonTransform.worldToLocalMatrix);
        _skyboxMaterial.SetMatrix("_MilkyWayWorld2Obj", _milkuWayTransform.worldToLocalMatrix);
    }

    private bool _changingTime = false;

    IEnumerator ChangeTime(bool toNight)
    {
        _changingTime = true;
        Light mainLight1 = toNight ? _moon : _sun;
        Light mainLight2 = toNight ? _sun : _moon;

        mainLight1.enabled = true;
        float updateTime = 0f;

        while (updateTime <= 1)
        {
            updateTime += Time.deltaTime;
            mainLight1.intensity = Mathf.Lerp(mainLight1.intensity, toNight ? 1.0f : 1.0f, updateTime);
            mainLight2.intensity = Mathf.Lerp(mainLight2.intensity, toNight ? 0.0f : 0.0f, updateTime);

            yield return 0;
        }

        // mainLight2.enabled = false;
        _changingTime = false;
    }

    public Vector3 GetPrincipalLightDirection()
    {
        if (_time < 6 || _time > 18)
        {
            return _moon.transform.forward;
        }
        else
        {
            return _sun.transform.forward;
        }
    }

    public Color GetPrincipalLightColor()
    {
        if (_time < 6 || _time > 18)
        {
            return _moon.color;
        }
        else
        {
            return _sun.color;
        }
    }

    void GenerateBlendProbe()
    {
        gameObject.GetComponent < ReflectionProbe > ().RenderProbe();
    }

    public float _planetRotationSpeed = 1.0f;
    private float _counting = 0.0f;
    private float _timeTillProbeUpdate = 0.1f;
    void Update()
    {
        if (!isPlaying)
        {
            var deltaTime = Time.deltaTime * _planetRotationSpeed;
            _time += deltaTime;
            /*
            if (_time > 11.9f && _time <= 12.1f)
            {
                _time = 12.1f;
            } else if (_time > 23.9f && _time <= 24.1f)
            {
                _time = 0.1f;
            } 
            */
            _counting += deltaTime;
        }

        //debug = _inverseTime;

        if(!_changingTime)
        {
            if(Mathf.Abs(_time - 6f) < 0.01f)
            {
                Debug.Log("Day");
                StartCoroutine(ChangeTime(false));
            }
            if(Mathf.Abs(_time - 18f) < 0.01f)
            {
                Debug.Log("Night");
                StartCoroutine(ChangeTime(true));
            }
        }

        _skyData = GetSkyColor(_time);
        TransformBodies();
        SetShaderProperties();

        if (_counting >= _timeTillProbeUpdate)
        {
            _counting = 0.0f;
            DynamicGI.UpdateEnvironment();
        }
        GenerateBlendProbe();
    }
}
