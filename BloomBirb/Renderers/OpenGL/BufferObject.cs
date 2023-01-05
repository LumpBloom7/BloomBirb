using Silk.NET.OpenGL;

namespace BloomBirb.Renderers.OpenGL;

//Our buffer object abstraction.
public class BufferObject<TDataType> : IDisposable
    where TDataType : unmanaged
{
    //Our handle, buffertype and the GL instance this class will use, these are private because they have no reason to be public.
    //Most of the time you would want to abstract items to make things like this invisible.
    private uint handle;
    private BufferTargetARB bufferType;
    private GL gl;

    public unsafe BufferObject(Span<TDataType> data, BufferTargetARB bufferType)
    {
        //Setting the gl instance and storing our buffer type.
        gl = OpenGLRenderer.GlContext;

        this.bufferType = bufferType;

        //Getting the handle, and then uploading the data to said handle.
        handle = gl.GenBuffer();
        Bind();
        fixed (void* d = data)
        {
            gl.BufferData(bufferType, (nuint)(data.Length * sizeof(TDataType)), d, BufferUsageARB.StaticDraw);
        }
    }

    public void Bind()
    {
        //Binding the buffer object, with the correct buffer type.
        gl.BindBuffer(bufferType, handle);
    }

    public void Dispose()
    {
        //Remember to delete our buffer.
        gl.DeleteBuffer(handle);
        GC.SuppressFinalize(this);
    }
}
