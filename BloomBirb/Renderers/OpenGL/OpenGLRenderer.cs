using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomBirb.Renderers.OpenGL;

public class OpenGLRenderer : IDisposable
{
    private static GL? glContext = null;

    private static DebugProc debugProcCallback = debugCallback;
    private static GCHandle debugProcCallbackHandle;

    public static GL Initialize(IWindow window)
    {
        glContext ??= GL.GetApi(window);

        debugProcCallbackHandle = GCHandle.Alloc(debugProcCallback);

        glContext.DebugMessageCallback(debugProcCallback, IntPtr.Zero);
        glContext.Enable(EnableCap.DebugOutput);
        glContext.Enable(EnableCap.DebugOutputSynchronous);

        unsafe
        {
            Console.WriteLine($"GL Version: {glContext.GetStringS(GLEnum.Version)}");
            Console.WriteLine($"GL Vendor: {glContext.GetStringS(GLEnum.Vendor)}");
            Console.WriteLine($"GL Renderer: {glContext.GetStringS(GLEnum.Renderer)}");

            int numExtensions = glContext.GetInteger(GLEnum.NumExtensions);
            Console.Write("GL Extensions: ");
            for (int i = 0; i < numExtensions; ++i)
                Console.Write($"{glContext.GetStringS(GLEnum.Extensions, (uint)i)} ");

            Console.WriteLine();
        }

        return glContext;
    }

    public static GL GlContext
    {
        get
        {
            ArgumentNullException.ThrowIfNull(glContext);
            return glContext;
        }
    }

    private static void debugCallback(GLEnum source, GLEnum type, int id, GLEnum severity, int length, IntPtr message, IntPtr userParam)
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
