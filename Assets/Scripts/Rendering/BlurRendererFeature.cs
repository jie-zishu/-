using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlurRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class BlurSettings
    {
        [Range(0f, 5f)] public float blurStrength = 1f;
        [Range(1, 8)] public int blurIterations = 3;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public static BlurRendererFeature Instance { get; private set; }

    public BlurSettings settings = new BlurSettings();
    private BlurRenderPass blurPass;

    public override void Create()
    {
        Instance = this;
        blurPass = new BlurRenderPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.blurStrength <= 0.001f) return;
        blurPass.Setup(settings.blurStrength, settings.blurIterations);
        renderer.EnqueuePass(blurPass);
    }

    public void SetBlurStrength(float value)
    {
        settings.blurStrength = value;
    }

    public float GetBlurStrength()
    {
        return settings.blurStrength;
    }

    protected override void Dispose(bool disposing)
    {
        blurPass?.Dispose();
    }

    // ---------------------------------------------------------------
    // Render Pass
    // ---------------------------------------------------------------
    private class BlurRenderPass : ScriptableRenderPass
    {
        private Material blurMaterial;
        private float blurStrength;
        private int blurIterations;

        private RTHandle cameraColorTarget;
        private RTHandle tempRT0;
        private RTHandle tempRT1;

        private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");

        public BlurRenderPass(BlurSettings settings)
        {
            this.renderPassEvent = settings.renderPassEvent;
            blurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/GaussianBlur"));
        }

        public void Setup(float strength, int iterations)
        {
            blurStrength = strength;
            blurIterations = iterations;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (blurMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("GaussianBlur");
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            RenderingUtils.ReAllocateIfNeeded(ref tempRT0, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempRT0");
            RenderingUtils.ReAllocateIfNeeded(ref tempRT1, desc, FilterMode.Bilinear, TextureWrapMode.Clamp, name: "_TempRT1");

            // First blit: camera → tempRT0
            Blitter.BlitCameraTexture(cmd, cameraColorTarget, tempRT0);

            for (int i = 0; i < blurIterations; i++)
            {
                float stepSize = blurStrength * (1f + i * 0.5f);
                cmd.SetGlobalFloat(BlurSizeId, stepSize);

                // Horizontal: tempRT0 → tempRT1
                Blitter.BlitCameraTexture(cmd, tempRT0, tempRT1, blurMaterial, 0);

                // Vertical: tempRT1 → tempRT0
                Blitter.BlitCameraTexture(cmd, tempRT1, tempRT0, blurMaterial, 1);
            }

            // Final blit: tempRT0 → camera target
            Blitter.BlitCameraTexture(cmd, tempRT0, cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public void Dispose()
        {
            tempRT0?.Release();
            tempRT1?.Release();
            CoreUtils.Destroy(blurMaterial);
        }
    }
}
