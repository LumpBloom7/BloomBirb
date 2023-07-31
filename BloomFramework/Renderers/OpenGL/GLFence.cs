using Silk.NET.OpenGL;

namespace BloomFramework.Renderers.OpenGL;

/// <summary>
/// A wrapper around an OpenGL Sync object
/// <br/>
/// This wrapper also provides methods to poll/wait for a signal.
/// </summary>
public sealed class GLFence : IDisposable
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
            if (signalled || isDisposed)
                return true;

            var result = renderer.Context.ClientWaitSync(fenceHandle, SyncObjectMask.Bit, (uint)TimeSpan.FromMilliseconds(1000).TotalNanoseconds);

            if (result is GLEnum.ConditionSatisfied or GLEnum.AlreadySignaled)
            {
                signalled = true;
                renderer.Context.DeleteSync(fenceHandle);
            }

            return signalled;
        }
    }

    public void WaitUntilSignalled()
    {
        while (!IsSignalled);
    }

    private bool isDisposed;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        if (signalled || isDisposed)
            return;

        renderer.Context.DeleteSync(fenceHandle);

        isDisposed = true;
    }
}
