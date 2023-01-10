using System.Runtime.InteropServices;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomBirb.Renderers.OpenGL;

public class OpenGLRenderer
{
    private static GL? glContext = null;

    public static GL CreateContext(IWindow window)
    {
        glContext ??= GL.GetApi(window);

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



}
