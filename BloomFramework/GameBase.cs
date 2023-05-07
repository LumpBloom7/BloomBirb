using System.Numerics;
using BloomFramework.Audio;
using BloomFramework.Graphics.Containers;
using BloomFramework.Renderers.OpenGL;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomFramework;

public class GameBase : Container
{
    private IWindow window;
    protected OpenGlRenderer renderer { get; private init; }

    public GameBase()
    {
        window = Window.Create(CreateWindowOptions());
        renderer = new OpenGlRenderer();

        window.Load += LoadInternal;
        window.Update += UpdateInternal;
        window.Render += _ => render();
        window.Resize += onResize;
        window.Closing += Dispose;

        // Shift origin to bottom left corner
        Position = new Vector2(-1);

        window.Run();
    }

    protected override void Load()
    {
        renderer.Initialize(window);
        renderer.Context?.Enable(EnableCap.DepthTest);
        renderer.Context?.DepthFunc(DepthFunction.Lequal);
        renderer.Context?.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

        onResize(window.Size); // Make sure we start off in a good state

        OpenAl.CreateContext();

        base.Load();
    }

    protected virtual WindowOptions CreateWindowOptions()
    {
       return WindowOptions.Default with
       {
           Size = new Vector2D<int>(1024, 768),
           Title = "Test",
           VSync = true,
           PreferredDepthBufferBits = 16,
           API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4,0))
       };
    }

    private void render()
    {
        renderer.BeginFrame();

        QueueDraw(renderer);

        renderer.EndFrame();
    }

    private void onResize(Vector2D<int> size)
    {
        renderer.Context?.Viewport(size);
        Size = new Vector2(size.X, size.Y);
        Scale = new Vector2(2f / size.X, 2f / size.Y);
    }
}
