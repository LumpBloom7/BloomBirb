using System.Numerics;
using BloomFramework.Audio;
using BloomFramework.Graphics.Containers;
using BloomFramework.Input;
using BloomFramework.Renderers.OpenGL;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomFramework;

public class GameBase : Container
{
    private IWindow window;
    protected OpenGlRenderer Renderer { get; private init; }
    protected InputManager Input { get; private set; }

    public GameBase()
    {
        window = Window.Create(CreateWindowOptions());
        window.Initialize();

        Renderer = new OpenGlRenderer(window);

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
        Input = new InputManager(window);

        Renderer.Context?.Enable(EnableCap.DepthTest);
        Renderer.Context?.DepthFunc(DepthFunction.Lequal);
        Renderer.Context?.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

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
            API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new APIVersion(4, 0))
        };
    }

    private void render()
    {
        Renderer.BeginFrame();

        QueueDraw(Renderer);

        Renderer.EndFrame();
    }

    private void onResize(Vector2D<int> size)
    {
        Renderer.Context?.Viewport(size);
        Size = new Vector2(size.X, size.Y);
        Scale = new Vector2(2f / size.X, 2f / size.Y);
    }
}
