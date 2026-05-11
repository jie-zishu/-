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
        settings.blurStrength = 0f;
        blurPass = new BlurRenderPass(settings);
    }

    protected override void Dispose(bool disposing)
    {
        Instance = null;
        blurPass?.Dispose();
        base.Dispose(disposing);
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

    // ---------------------------------------------------------------
    // Render Pass
    // ---------------------------------------------------------------
    private class BlurRenderPass : ScriptableRenderPass
    {
        private Material blurMaterial;
        private float blurStrength;
        private int blurIterations;

        private static readonly int BlurSizeId = Shader.PropertyToID("_BlurSize");
        private static readonly int TempRT0Id = Shader.PropertyToID("_TempRT0");
        private static readonly int TempRT1Id = Shader.PropertyToID("_TempRT1");
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");

        public BlurRenderPass(BlurSettings settings)
        {
            this.renderPassEvent = settings.renderPassEvent;
            blurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/GaussianBlur"));
            if (blurMaterial == null)
                Debug.LogError("[GaussianBlur] Shader 'Hidden/GaussianBlur' not found!");
        }

        public void Setup(float strength, int iterations)
        {
            blurStrength = strength;
            blurIterations = iterations;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            // Use classic cmd.GetTemporaryRT — reliable across URP versions
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            cmd.GetTemporaryRT(TempRT0Id, desc);
            cmd.GetTemporaryRT(TempRT1Id, desc);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (blurMaterial == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("GaussianBlur");

            // Copy camera target → tempRT0 (cast RTHandle to RenderTargetIdentifier)
            var source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            cmd.Blit((RenderTargetIdentifier)source, TempRT0Id);

            // Blur passes (cmd.Blit auto-binds source as _MainTex)
            for (int i = 0; i < blurIterations; i++)
            {
                float stepSize = blurStrength * (1f + i * 0.5f);
                cmd.SetGlobalFloat(BlurSizeId, stepSize);

                cmd.Blit(TempRT0Id, TempRT1Id, blurMaterial, 0); // Horizontal
                cmd.Blit(TempRT1Id, TempRT0Id, blurMaterial, 1); // Vertical
            }

            // Final: RT0 → camera target
            cmd.Blit(TempRT0Id, source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(TempRT0Id);
            cmd.ReleaseTemporaryRT(TempRT1Id);
        }

        public void Dispose()
        {
            CoreUtils.Destroy(blurMaterial);
        }
    }
}
