﻿using System.Diagnostics;
using System.Runtime.InteropServices;
using BloomBirb.Graphics;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL.Batches;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomBirb.Renderers.OpenGL;

public class OpenGLRenderer : IDisposable
{
    public GL? Context = null;

    private static DebugProc debugProcCallback = debugCallback;
    private static GCHandle debugProcCallbackHandle;

    private static uint[] textureUnits = new uint[1];
    private static uint boundProgram;

    private bool isInitialized = false;

    public Texture? BlankTexture { get; private set; }

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

        BlankTexture = new Texture(this);

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

    public void BindShader(uint programID)
    {
        if (boundProgram == programID)
            return;

        boundProgram = programID;
        Context?.UseProgram(programID);
    }

    public Texture CreateTexture(Stream? stream) => new(this, stream);

    public void BindTexture(uint textureHandle, TextureUnit textureUnit = 0)
    {
        int textureUnitIndex = textureUnit - TextureUnit.Texture0;
        if (textureUnits[textureUnitIndex] == textureHandle)
            return;

        textureUnits[textureUnitIndex] = textureHandle;
        Context?.ActiveTexture(textureUnit);
        Context?.BindTexture(TextureTarget.Texture2D, textureHandle);
    }

    public void Flush() => Context?.Flush();

    // <Render>

    // This is the current active batch that the next vertex will be submitted to
    private IVertexBatch? currentVertexBatch;

    // All the batches that have been initialized so far
    private Dictionary<Type, IVertexBatch> defaultBatches = new();

    // Attempts to initialize or reuse a batch we've already created
    public void UseBatch<BatchType>() where BatchType : IVertexBatch, new()
    {
        if (currentVertexBatch is BatchType)
            return;

        if (!defaultBatches.TryGetValue(typeof(BatchType), out IVertexBatch? batch))
        {
            batch = new BatchType();
            batch.Initialize(this, 10000, 100);
            defaultBatches.Add(typeof(BatchType), batch);
        }

        currentVertexBatch?.FlushBatch();
        currentVertexBatch = batch;
    }

    private Stack<Drawable> deferredDrawables = new();

    public void BeginFrame()
    {
        Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        DrawDepth.Reset();
    }

    public void QueueDrawable(Drawable drawable, bool IsTranslucent = false)
    {
        drawable.DrawDepth = DrawDepth.NextDepth;
        DrawDepth.Increment();

        if (IsTranslucent)
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
        while (deferredDrawables.Count > 0)
            deferredDrawables.Pop().Draw(this);

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
