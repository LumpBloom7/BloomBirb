using System.Diagnostics;
using System.Runtime.InteropServices;
using BloomBirb.Graphics;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL.Batches;
using BloomBirb.Renderers.OpenGL.Textures;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Texture = BloomBirb.Renderers.OpenGL.Textures.Texture;

namespace BloomBirb.Renderers.OpenGL;

public class OpenGlRenderer : IDisposable
{
    public GL Context { get; private set; } = null!;

    private static readonly DebugProc debug_proc_callback = debugCallback;
    private static GCHandle? debugProcCallbackHandle;

    private static readonly Texture?[] texture_units = new Texture[1];
    private static uint boundProgram;

    private bool isInitialized;

    public TextureWhitePixel BlankTexture { get; private set; } = null!;

    public void Initialize(IWindow window)
    {
        if (isInitialized)
            return;

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

        isInitialized = true;
    }

    [Conditional("DEBUG")]
    private void enableDebugMessageCallback()
    {
        debugProcCallbackHandle = GCHandle.Alloc(debug_proc_callback);
        Context.DebugMessageCallback(debug_proc_callback, nint.Zero);
        Context.Enable(EnableCap.DebugOutput);
        Context.Enable(EnableCap.DebugOutputSynchronous);
    }

    private void ensureInitialized()
    {
        if (!isInitialized)
            throw new InvalidOperationException("OpenGLRenderer is not initialized");
    }

    private void clear(ClearBufferMask bufferMask) => Context.Clear((uint)bufferMask);

    public Shader CreateShader(params uint[] shaderParts) => new(this, shaderParts);

    public uint CreateShaderPart(ShaderType type, string source)
    {
        ensureInitialized();

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

        currentVertexBatch?.FlushBatch();

        boundProgram = programId;
        Context.UseProgram(programId);
    }

    public void BindTexture(Texture texture, TextureUnit textureUnit = TextureUnit.Texture0)
    {
        int textureUnitIndex = textureUnit - TextureUnit.Texture0;
        if (texture_units[textureUnitIndex] == texture)
            return;

        currentVertexBatch?.FlushBatch();

        texture_units[textureUnitIndex] = texture;

        // TODO: THIS IS A QUICK BODGE TO TEST THINGS, SEE RELATED UNIFORM IN SHADERS/SHARED.h
        Context.Uniform2(34, new System.Numerics.Vector2(texture.TextureSize.Width, texture.TextureSize.Height));
        Context.ActiveTexture(textureUnit);
        Context.BindTexture(TextureTarget.Texture2D, texture.TextureHandle);
    }

    public ITexture? GetBoundTexture(TextureUnit textureUnit = TextureUnit.Texture0) => texture_units[0];


    // <Render>
    private readonly DrawableBatchTree batchTree = new();

    // This is the current active batch that the next vertex will be submitted to
    private IVertexBatch? currentVertexBatch;

    // All the batches that have been initialized so far
    private readonly Dictionary<Type, IVertexBatch> defaultBatches = new();

    private readonly List<IVertexBatch> usedBatches = new();

    // Attempts to initialize or reuse a batch we've already created
    public void UseBatch<TBatchType>() where TBatchType : IVertexBatch, new()
    {
        if (currentVertexBatch is TBatchType)
            return;

        if (!defaultBatches.TryGetValue(typeof(TBatchType), out IVertexBatch? batch))
        {
            batch = new TBatchType();
            batch.Initialize(this, 1000, 1000);
            defaultBatches[typeof(TBatchType)] = batch;
        }

        currentVertexBatch?.FlushBatch();
        currentVertexBatch = batch;
        usedBatches.Add(batch);
    }

    private readonly Stack<Drawable> deferredDrawables = new();

    public void BeginFrame()
    {
        Context.DepthMask(true);
        Context.Disable(EnableCap.Blend);
        clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawDepth.Reset();

        foreach (var batch in usedBatches)
            batch.ResetBatch();

        usedBatches.Clear();

        currentVertexBatch = null;
    }

    public void QueueDrawable(Drawable drawable)
    {
        drawable.DrawDepth = DrawDepth.NextDepth;
        DrawDepth.Increment();

        if (Math.Abs(DrawDepth.NextDepth - 1.001f) < float.Epsilon)
            batchTree.DrawAll(this);

        if (drawable.IsTranslucent || DrawDepth.NextDepth > 1)
        {
            deferredDrawables.Push(drawable);
            return;
        }

        drawable.Draw(this);
    }

    public void AddVertex<TVertexType>(TVertexType vertex)
        where TVertexType : unmanaged, IEquatable<TVertexType>, IVertex
    {
        ((IVertexBatch<TVertexType>)currentVertexBatch!).AddVertex(vertex);
    }

    public void EndFrame()
    {
        batchTree.DrawAll(this);

        if (deferredDrawables.Count > 0)
        {
            currentVertexBatch?.FlushBatch();
            Context.Enable(EnableCap.Blend);
            Context.DepthMask(false);
            while (deferredDrawables.Count > 0)
                deferredDrawables.Pop().Draw(this);
        }

        currentVertexBatch?.FlushBatch();
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
