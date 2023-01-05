using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomBirb.Graphics;

public class OpenGLRenderer
{
    private static GL? glContext = null;

    public static GL CreateContext(IWindow window)
    {
        return glContext ??= GL.GetApi(window);
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
