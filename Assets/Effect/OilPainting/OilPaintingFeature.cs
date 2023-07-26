using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OilPaintingFeature : ScriptableRendererFeature
{
    public int radius = 3;
    public float intensity = 20f;

    #region Renderer Pass
    public class OilPaintingPass : ScriptableRenderPass
    {
        ComputeShader _filterComputeShader;
        string _kernelName;
        int _renderIdDst;
        int _renderIdSrc;

        RenderTargetIdentifier _renderTargetIdentifierDst;
        RenderTargetIdentifier _renderTargetIdentifierSrc;
        int _renderTextureWidth;
        int _renderTextureHeight;

        public static int _radius;
        public static float _intensity;

        public OilPaintingPass(ComputeShader filterComputeShader, string kernelName, int idSrc,int idDst,int radius, float intensity)
        {
            _filterComputeShader = filterComputeShader;
            _kernelName = kernelName;
            _renderIdSrc = idSrc;
            _renderIdDst = idDst;
            _radius = radius;
            _intensity = intensity;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
            var renderTextureDescriptor = new RenderTextureDescriptor(cameraTargetDescriptor.width, cameraTargetDescriptor.height);
            var rtdCam = new RenderTextureDescriptor(cameraTargetDescriptor.width, cameraTargetDescriptor.height);
            renderTextureDescriptor.msaaSamples = 1;
            renderTextureDescriptor.enableRandomWrite = true;
            rtdCam.msaaSamples = 1;
            rtdCam.enableRandomWrite = true;
            cmd.GetTemporaryRT(_renderIdDst, renderTextureDescriptor);
            cmd.GetTemporaryRT(_renderIdSrc, rtdCam);
            _renderTargetIdentifierDst = new RenderTargetIdentifier(_renderIdDst);
            _renderTargetIdentifierSrc = new RenderTargetIdentifier(_renderIdSrc);
            _renderTextureWidth = renderTextureDescriptor.width;
            _renderTextureHeight = renderTextureDescriptor.height;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (renderingData.cameraData.isSceneViewCamera)
                return;

            CommandBuffer cmd = CommandBufferPool.Get();
            var mainKernel = _filterComputeShader.FindKernel(_kernelName);
            cmd.Blit(renderingData.cameraData.targetTexture, _renderTargetIdentifierSrc);
            cmd.SetComputeTextureParam(_filterComputeShader, mainKernel, _renderIdSrc, _renderTargetIdentifierSrc);
            cmd.SetComputeTextureParam(_filterComputeShader, mainKernel, _renderIdDst, _renderTargetIdentifierDst);
            cmd.SetComputeIntParam(_filterComputeShader, "width", _renderTextureWidth);
            cmd.SetComputeIntParam(_filterComputeShader, "height", _renderTextureHeight);
            cmd.SetComputeIntParam(_filterComputeShader, "radius", _radius);
            cmd.SetComputeFloatParam(_filterComputeShader, "intensity", _intensity);
            cmd.DispatchCompute(_filterComputeShader, mainKernel, Mathf.CeilToInt(_renderTextureWidth / 8.0f), Mathf.CeilToInt(_renderTextureHeight / 8.0f), 1);
            cmd.Blit(_renderTargetIdentifierDst, renderingData.cameraData.renderer.cameraColorTarget);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(_renderIdSrc);
            cmd.ReleaseTemporaryRT(_renderIdDst);
        }
    }

    #endregion

    #region Renderer Feature

    OilPaintingPass _scriptablePass;
    bool _initialized;

    public ComputeShader FilterComputeShader;
    public string KernelName = "OilPainting";

    public override void Create()
    {
        if (FilterComputeShader == null)
        {
            _initialized = false;
            return;
        }

        _scriptablePass = new OilPaintingPass(FilterComputeShader, KernelName,Shader.PropertyToID("_OriginTex"), Shader.PropertyToID("_Result"),radius,intensity);
        _scriptablePass.renderPassEvent = RenderPassEvent.AfterRendering;
        _initialized = true;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (_initialized)
        {
            renderer.EnqueuePass(_scriptablePass);
        }
    }

    #endregion
}