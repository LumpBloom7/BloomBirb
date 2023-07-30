using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL;
public class GLFence
{
    public GLFence(OpenGlRenderer renderer)
    {
        this.renderer = renderer;
        fenceHandle = renderer.Context.FenceSync(SyncCondition.SyncGpuCommandsComplete, SyncBehaviorFlags.None);
    }

    private readonly OpenGlRenderer renderer;

    private readonly nint fenceHandle;

    private bool signalled;

    public bool IsSignalled
    {
        get
        {
            if (signalled)
                return true;

            var result = renderer.Context.ClientWaitSync(fenceHandle, SyncObjectMask.Bit, (uint)TimeSpan.FromMilliseconds(500).TotalNanoseconds);

            if(result is GLEnum.ConditionSatisfied or GLEnum.AlreadySignaled){
                signalled = true;
                renderer.Context.DeleteSync(fenceHandle);
            }

            return signalled;
        }
    }
}
