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

public class OpenGLRenderer : IDisposable
{
    public GL? Context = null;

    private static DebugProc debugProcCallback = debugCallback;
    private static GCHandle debugProcCallbackHandle;

    private static Texture[] textureUnits = new Texture[1];
    private static uint boundProgram;

    private bool isInitialized = false;

    public TextureWhitePixel BlankTexture { get; private set; } = null!;

    public void Initialize(IWindow window)
    {
        if (isInitialized)
            return;

        Context ??= GL.GetApi(window);

        debugProcCallbackHandle = GCHandle.Alloc(debugProcCallback);

        Context.DebugMessageCallback(debugProcCallback, nint.Zero);
        Context.Enable(EnableCap.DebugOutput);
        Context.Enable(EnableCap.DebugOutputSynchronous);

        unsafe
        {
            Console.WriteLine($"GL Version: {Context.GetStringS(GLEnum.Version)}");
            Console.WriteLine($"GL Vendor: {Context.GetStringS(GLEnum.Vendor)}");
            Console.WriteLine($"GL Renderer: {Context.GetStringS(GLEnum.Renderer)}");

            int numExtensions = Context.GetInteger(GLEnum.NumExtensions);
            Console.Write("GL Extensions: ");
            for (int i = 0; i < numExtensions; ++i)
                Console.Write($"{Context.GetStringS(GLEnum.Extensions, (uint)i)} ");

            Console.WriteLine();
        }

        BlankTexture = new TextureWhitePixel(this);

        isInitialized = true;
    }

    private void ensureInitialized()
    {
        if (!isInitialized)
            throw new InvalidOperationException("OpenGLRenderer is not initialized");
    }

    public void Clear(ClearBufferMask bufferMask) => Context?.Clear((uint)bufferMask);

    public Shader CreateShader(params uint[] shaderParts) => new(this, shaderParts);

    public uint CreateShaderPart(ShaderType type, string source)
    {
        ensureInitialized();
        Debug.Assert(Context is not null);

        uint handle = Context.CreateShader(type);

        Context.ShaderSource(handle, source);
        Context.CompileShader(handle);

        //Check for linking errors.
        string status = Context.GetShaderInfoLog(handle);
        if (!string.IsNullOrEmpty(status))
            throw new Exception($"Program failed to link with error: {status}");

        return handle;
    }
    private uint currentVBO;

    public bool BindBuffer(uint buffer)
    {
        if (currentVBO == buffer)
            return false;

        currentVBO = buffer;
        Context?.BindBuffer(BufferTargetARB.ArrayBuffer, buffer);
        return true;
    }

    private uint currentVAO;

    public bool BindVertexArray(uint vaoHandle)
    {
        if (currentVAO == vaoHandle)
            return false;

        currentVAO = vaoHandle;
        Context?.BindVertexArray(vaoHandle);
        return true;
    }

    public void BindShader(uint programID)
    {
        if (boundProgram == programID)
            return;

        currentVertexBatch?.FlushBatch();

        boundProgram = programID;
        Context?.UseProgram(programID);
    }

    public void BindTexture(Texture texture, TextureUnit textureUnit = TextureUnit.Texture0)
    {
        int textureUnitIndex = textureUnit - TextureUnit.Texture0;
        if (textureUnits[textureUnitIndex] == texture)
            return;

        currentVertexBatch?.FlushBatch();

        textureUnits[textureUnitIndex] = texture;

        // TODO: THIS IS A QUICK BODGE TO TEST THINGS, SEE RELATED UNIFORM IN SHADERS/SHARED.h
        Context?.Uniform2(34, new System.Numerics.Vector2(texture.TextureSize.Width, texture.TextureSize.Height));
        Context?.ActiveTexture(textureUnit);
        Context?.BindTexture(TextureTarget.Texture2D, texture.TextureHandle);
    }

    public ITexture? GetBoundTexture(TextureUnit textureUnit = TextureUnit.Texture0) => textureUnits[0];

    public void Flush() => Context?.Flush();

    // <Render>

    private DrawableBatchTree batchTree = new();

    // This is the current active batch that the next vertex will be submitted to
    private IVertexBatch? currentVertexBatch;

    // All the batches that have been initialized so far
    private Dictionary<Type, IVertexBatch> defaultBatches = new();

    private List<IVertexBatch> usedBatches = new();

    // Attempts to initialize or reuse a batch we've already created
    public void UseBatch<BatchType>() where BatchType : IVertexBatch, new()
    {
        if (currentVertexBatch is BatchType)
            return;

        if (!defaultBatches.TryGetValue(typeof(BatchType), out IVertexBatch? batch))
        {
            batch = new BatchType();
            batch.Initialize(this, 1000, 1000);
            defaultBatches[typeof(BatchType)] = batch;
        }

        currentVertexBatch?.FlushBatch();
        currentVertexBatch = batch;
        usedBatches.Add(batch);
    }

    private Stack<Drawable> deferredDrawables = new();

    public void BeginFrame()
    {
        Context?.DepthMask(true);
        Context?.Disable(EnableCap.Blend);
        Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawDepth.Reset();

        foreach (var batch in usedBatches)
            batch.ResetBatch();

        usedBatches.Clear();

        currentVertexBatch = null;
    }

    public void QueueDrawable(Drawable drawable, Shader shader, TextureUsage texture, bool IsTranslucent = false)
    {
        drawable.DrawDepth = DrawDepth.NextDepth;
        DrawDepth.Increment();

        if (DrawDepth.NextDepth == 1.001f)
            batchTree.DrawAll(this);

        if (IsTranslucent || DrawDepth.NextDepth > 1)
        {
            deferredDrawables.Push(drawable);
            return;
        }

        drawable.Draw(this);
    }

    public void AddVertex<VertexType>(VertexType vertex)
        where VertexType : unmanaged, IEquatable<VertexType>, IVertex
    {
        ((IVertexBatch<VertexType>)currentVertexBatch!).AddVertex(vertex);
    }

    public void EndFrame()
    {
        batchTree.DrawAll(this);

        if (deferredDrawables.Count > 0)
        {
            currentVertexBatch?.FlushBatch();
            Context?.Enable(EnableCap.Blend);
            Context?.DepthMask(false);
            while (deferredDrawables.Count > 0)
                deferredDrawables.Pop().Draw(this);
        }

        currentVertexBatch?.FlushBatch();
    }

    // </render>

    private static void debugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, nint message, nint userParam)
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

        debugProcCallbackHandle.Free();
        isDisposed = true;
        GC.SuppressFinalize(this);
    }

    ~OpenGLRenderer()
    {
        Dispose();
    }
}
