namespace UnityEngine.Rendering.Universal
{
    public class CloudsFeature : DrawFullscreenFeature
    {
        public CloudSimulation _clouds;
        public NoiseSimulator _noiseSimulator;

        public GradientSimulation _gradientSimulation;

        private void LocateComponents()
        {
            _clouds = FindObjectOfType<CloudSimulation>();
            if (_clouds == null)
                Debug.LogError("No CloudSimulation found in the scene!");
        
            _noiseSimulator = FindObjectOfType<NoiseSimulator>();
            if(_noiseSimulator == null)
                Debug.LogError("No NoiseSimulator found in the scene!");

            _gradientSimulation = FindObjectOfType<GradientSimulation>();
            if(_gradientSimulation == null)
                Debug.LogError("No GradientSimulation found in the scene!");
        }
        
        private void CheckComponents()
        {
            if (_clouds == null||_noiseSimulator == null)
                LocateComponents();
        }

        public override void Create()
        {
            base.Create();

            LocateComponents();
        }

        private void UpdateEverything()
        {
            CheckComponents();

            //_noiseSimulator._simulate = true;
            _noiseSimulator.UpdateNoise();
            if (!Application.isPlaying)
                _gradientSimulation.UpdateGradient();
            //settings.blitMaterial.SetTexture("VolumeNoise", _noiseSimulator._volumeTexture);
            settings.renderPassEvent = (Camera.main.transform.position.y > 24.9f) ? RenderPassEvent.AfterRenderingTransparents : RenderPassEvent.AfterRenderingSkybox;
            settings.blitMaterial.SetTexture("VolumeNoise", _noiseSimulator._volumeTexture);
            settings.blitMaterial.SetTexture("DetailNoise", _noiseSimulator._detailTexture);
            settings.blitMaterial.SetTexture("PrecalcNoise", _clouds._precalcNoise);
            settings.blitMaterial.SetTexture("GradientNoise", _gradientSimulation._gradientTexture);

            settings.blitMaterial.SetVector("_CloudOffset", _clouds._cloudOffset);
            settings.blitMaterial.SetFloat("_CloudScale", _clouds._cloudScale);
            settings.blitMaterial.SetFloat("_DensityThreshold", _clouds._densityThreshold);
            settings.blitMaterial.SetFloat("_DensityMultiplier", _clouds._densityMultiplier);
            settings.blitMaterial.SetFloat("_DetailNoiseMultiplier", _clouds._detailMultiplier); 
            settings.blitMaterial.SetFloat("_ContainerEdgeFadeDst", _clouds._containerEdgeFadeDistance);
            settings.blitMaterial.SetInt("_NumStepsLight", _clouds._numSteps);

            settings.blitMaterial.SetVector("_PhaseParams", new Vector4(_clouds._fwdScattering, _clouds._bckScattering, _clouds._bareBrightness, _clouds._phaseFactor));
            settings.blitMaterial.SetVector("_DetailNoiseWeight", _clouds._detailNoiseWeight);
            settings.blitMaterial.SetVector("_VolumeNoiseWeight", _clouds._volumeNoiseWeight);
            settings.blitMaterial.SetFloat("_LightAbsortionSun", _clouds._lightAbsortionSun);
            settings.blitMaterial.SetFloat("_LightAbsortionCloud", _clouds._lightAbsortionCloud);
            settings.blitMaterial.SetFloat("_DarknessThreshold", _clouds._darknessThreshold);
            settings.blitMaterial.SetFloat("_PrecalcNoiseStrenght", _clouds._precalcNoiseStrenght);
            

            settings.blitMaterial.SetVector("_BoundsMin", _clouds.MinBounds);
            settings.blitMaterial.SetVector("_BoundsMax", _clouds.MaxBounds);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            UpdateEverything();

            blitPass.renderPassEvent = settings.renderPassEvent;
            blitPass.settings = settings;
            renderer.EnqueuePass(blitPass);
        }
    }
}