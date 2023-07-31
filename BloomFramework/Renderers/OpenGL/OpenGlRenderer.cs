using System.Diagnostics;
using System.Runtime.InteropServices;
using BloomFramework.Graphics;
using BloomFramework.Graphics.Vertices;
using BloomFramework.Renderers.OpenGL.Buffers;
using BloomFramework.Renderers.OpenGL.Textures;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomFramework.Renderers.OpenGL;

public class OpenGlRenderer : IDisposable
{
    public GL Context { get; private set; } = null!;

    private static readonly DebugProc debug_proc_callback = debugCallback;
    private static GCHandle? debugProcCallbackHandle;

    private static readonly Dictionary<TextureUnit, ITexture> texture_units = new();
    private static uint boundProgram;

    public TextureWhitePixel BlankTexture { get; private set; } = null!;

    public OpenGlRenderer(IWindow window)
    {
        Context = GL.GetApi(window);

        enableDebugMessageCallback();

        Console.WriteLine($"GL Version: {Context.GetStringS(GLEnum.Version)}");
        Console.WriteLine($"GL Vendor: {Context.GetStringS(GLEnum.Vendor)}");
        Console.WriteLine($"GL Renderer: {Context.GetStringS(GLEnum.Renderer)}");

        int numExtensions = Context.GetInteger(GLEnum.NumExtensions);
        Console.Write("GL Extensions: ");
        for (int i = 0; i < numExtensions; ++i)
            Console.Write($"{Context.GetStringS(GLEnum.Extensions, (uint)i)} ");

        Console.WriteLine();

        BlankTexture = new TextureWhitePixel(this);
    }

    [Conditional("DEBUG")]
    private void enableDebugMessageCallback()
    {
        debugProcCallbackHandle = GCHandle.Alloc(debug_proc_callback);
        Context.DebugMessageCallback(debug_proc_callback, nint.Zero);
        Context.Enable(EnableCap.DebugOutput);
        Context.Enable(EnableCap.DebugOutputSynchronous);
    }

    private void clear(ClearBufferMask bufferMask) => Context.Clear((uint)bufferMask);

    public Shader CreateShader(params uint[] shaderParts) => new(this, shaderParts);

    public uint CreateShaderPart(ShaderType type, string source)
    {
        uint handle = Context.CreateShader(type);

        Context.ShaderSource(handle, source);
        Context.CompileShader(handle);

        //Check for linking errors.
        string status = Context.GetShaderInfoLog(handle);
        if (!string.IsNullOrEmpty(status))
            throw new Exception($"Program failed to link with error: {status}");

        return handle;
    }

    private uint currentVbo;

    public bool BindBuffer(uint buffer)
    {
        if (currentVbo == buffer)
            return false;

        currentVbo = buffer;
        Context.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);
        return true;
    }

    private uint currentVao;

    public bool BindVertexArray(uint vaoHandle)
    {
        if (currentVao == vaoHandle)
            return false;

        currentVao = vaoHandle;
        Context.BindVertexArray(vaoHandle);
        return true;
    }

    public void BindShader(uint programId)
    {
        if (boundProgram == programId)
            return;

        currentVertexBuffer?.Draw();
        WaitForFences();

        boundProgram = programId;
        Context.UseProgram(programId);
    }

    public void BindTexture(ITexture texture, TextureUnit textureUnit = TextureUnit.Texture0)
    {
        if (texture_units.TryGetValue(textureUnit, out ITexture? previousTexture) && previousTexture.Equals(texture))
            return;

        currentVertexBuffer?.Draw();
        WaitForFences();

        texture_units[textureUnit] = texture;

        Context.ActiveTexture(textureUnit);
        Context.BindTexture(TextureTarget.Texture2D, texture.TextureHandle);
    }

    public ITexture? GetBoundTexture(TextureUnit textureUnit = TextureUnit.Texture0)
    {
        texture_units.TryGetValue(textureUnit, out ITexture? previousTexture);

        return previousTexture;
    }

    private GLFence? lastFence;

    /// <summary>
    /// Creates an OpenGL synchronisation object within a wrapper class that provides convenience methods for polling/waiting for a signal.
    /// <br/>
    /// This is really only needed if you're working in a context where explicit synchronisation is required.
    /// </summary>
    public GLFence CreateFence() => lastFence = new GLFence(this);

    public void WaitForFences()
    {
        if (lastFence is null)
            return;

        lastFence.WaitUntilSignalled();
    }

    // <Render>

    // This is the current active batch that the next vertex will be submitted to
    private IVertexBuffer? currentVertexBuffer;

    // All the batches that have been initialized so far
    private readonly Dictionary<Type, IVertexBuffer> defaultBuffers = new();

    private readonly List<IVertexBuffer> usedBuffers = new();

    // Attempts to initialize or reuse a batch we've already created
    public void UseBuffer<TVertexBuffer>() where TVertexBuffer : IVertexBuffer
    {
        if (currentVertexBuffer is TVertexBuffer)
            return;

        if (!defaultBuffers.TryGetValue(typeof(TVertexBuffer), out IVertexBuffer? buffer))
        {
            buffer = TVertexBuffer.Create(this, 10000);
            defaultBuffers[typeof(TVertexBuffer)] = buffer;
        }

        buffer.Draw();
        currentVertexBuffer = buffer;
        usedBuffers.Add(buffer);
    }

    private readonly Stack<Drawable> deferredDrawables = new();

    public void BeginFrame()
    {
        Context.DepthMask(true);
        Context.Disable(EnableCap.Blend);
        clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        DrawDepth.Reset();

        foreach (var batch in usedBuffers)
            batch.Reset();

        usedBuffers.Clear();
        currentVertexBuffer = null;
    }

    public void QueueDrawable(Drawable drawable)
    {
        drawable.DrawDepth = DrawDepth.NextDepth;
        DrawDepth.Increment();

        if (drawable.IsTranslucent || DrawDepth.NextDepth > 1)
        {
            deferredDrawables.Push(drawable);
            return;
        }
        drawable.Draw(this);
    }

    public void AddVertex<TVertex>(TVertex vertex) where TVertex : unmanaged, IEquatable<TVertex>, IVertex
    {
        ((IVertexBuffer<TVertex>)currentVertexBuffer!).AddVertex(ref vertex);
    }

    public void EndFrame()
    {
        if (deferredDrawables.Count > 0)
        {
            currentVertexBuffer?.Draw();
            Context.Enable(EnableCap.Blend);
            Context.DepthMask(false);
            while (deferredDrawables.Count > 0)
                deferredDrawables.Pop().Draw(this);

            currentVertexBuffer?.Draw();
        }
    }

    // </render>

    private static void debugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message,
        nint userParam)
    {
        string messageString = Marshal.PtrToStringAnsi(message, length);
        Console.WriteLine($"{severity} {type} | {messageString}");

        if (type == GLEnum.DebugTypeError)
            throw new Exception(messageString);
    }

    private bool isDisposed;

    public void Dispose()
    {
        if (isDisposed)
            return;

        debugProcCallbackHandle?.Free();
        isDisposed = true;
        GC.SuppressFinalize(this);
    }

    ~OpenGlRenderer()
    {
        Dispose();
    }
}
