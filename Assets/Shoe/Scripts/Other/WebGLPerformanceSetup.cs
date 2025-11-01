using UnityEngine;
#if UNITY_RENDER_PIPELINE_UNIVERSAL
using UnityEngine.Rendering.Universal;
#endif

public class WebGLPerformanceSetup : MonoBehaviour
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplyWebGLOptimizations()
    {
#if UNITY_WEBGL
        Application.targetFrameRate = 60;
        Application.runInBackground = false;

        QualitySettings.vSyncCount = 0;
        QualitySettings.antiAliasing = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.softParticles = false;
        QualitySettings.shadows = ShadowQuality.HardOnly;
        QualitySettings.shadowDistance = 35f;
        QualitySettings.shadowResolution = ShadowResolution.Low;
        QualitySettings.shadowCascades = 2;

        Time.fixedDeltaTime = 0.033f;
        Time.maximumDeltaTime = 0.1f;

#if UNITY_RENDER_PIPELINE_UNIVERSAL
        if (GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urp)
        {
            urp.supportsHDR = false;
            urp.msaaSampleCount = 0;
            urp.supportsCameraDepthTexture = false;
            urp.supportsOpaqueTexture = false;
            urp.shadowDistance = 35f;
            urp.shadowCascadeCount = 2;
            urp.shadowDepthBias = 1.0f;
            urp.shadowNormalBias = 1.0f;
            urp.renderScale = 0.85f;
        }
#endif

        Debug.Log("[WebGLPerformanceSetup] Optimizations with lightweight shadows applied");
#endif
    }
}
