using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BlitMaterialFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        private string profilingname;
        private int materialPassIndex;
        private Material material;
        private RenderTargetIdentifier source; //RenderTargetIdentifier points to a texture directly
        private RenderTargetHandle tempTexture; //RenderTargetHandle Points to a texture variable in a shader
        public CustomRenderPass(string _profilingname, Material _m, int _materialPassIndex) : base()
        {
            profilingname = _profilingname;
            material = _m;
            materialPassIndex = _materialPassIndex;
            tempTexture.Init("_TempDesaturateTexture");
        }
        public void SetSource(RenderTargetIdentifier _source)
        {
            source = _source;
        }

        // Here you can implement the rendering logic.
        // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get("SimpleDesaturateFeature");
            
            RenderTextureDescriptor cameraTextureDesc = renderingData.cameraData.cameraTargetDescriptor;
            cameraTextureDesc.depthBufferBits = 0; //We want the temporary texture to render as the same format as camera but with no depth buffer

            cmd.GetTemporaryRT(tempTexture.id, cameraTextureDesc, FilterMode.Bilinear); //gets a temporary texture, assigns it to temptex handle
            //                                                                            now the handle contains a texture we can render to


            Blit(cmd, source, tempTexture.Identifier(), material, 0); //runs the desaturate cmd on source tex and save it to temp tex
            Blit(cmd, tempTexture.Identifier(), source);              //runs the desaturate cmd on temp tex and save it back to source tex  
            //                                                          you cannot read and write to same tes at once

            context.ExecuteCommandBuffer(cmd); //lets Unity knows, the command has been completed so it can continues the cycle
            CommandBufferPool.Release(cmd); 
        }

        public override void FrameCleanup(CommandBuffer cmd) //unity calls this when it has finished rendering
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }

//----------------------------------------------------------------------------------------------------------
    [System.Serializable] public class Settings
    {
        public Material material;
        public int materialPassIndex = -1; //-1 means render all passes
        public RenderPassEvent renderEvent = RenderPassEvent.AfterRenderingOpaques;
    }
    [SerializeField] private Settings settings = new Settings();
//------------------------------------------------------------------------------------------------------------
    private CustomRenderPass m_ScriptablePass;

    public Material Material { get => settings.material;}

    /// <inheritdoc/>
    public override void Create()
    {
        //var material = new Material(Shader.Find("Shader Graphs/DesaturateGraph"))

        m_ScriptablePass = new CustomRenderPass(name, settings.material, settings.materialPassIndex);
        m_ScriptablePass.renderPassEvent = settings.renderEvent;

        // Configures where the render pass should be injected.
        m_ScriptablePass.renderPassEvent = settings.renderEvent;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.SetSource(renderer.cameraColorTarget);
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


