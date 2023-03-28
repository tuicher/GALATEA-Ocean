namespace UnityEngine.Rendering.Universal
{
    public class CloudsFeature : DrawFullscreenFeature
    {
        public CloudSimulation _clouds;
        public override void Create()
        {
            base.Create();

            _clouds = FindObjectOfType<CloudSimulation>();
            if (_clouds == null)
            {
                Debug.LogError("No ImageEffectTest found in the scene!");
            }
        }

        private void UpdateEverything()
        {
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