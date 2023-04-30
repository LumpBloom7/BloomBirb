using System.Numerics;
using BloomBirb.Audio;
using BloomBirb.Extensions;
using BloomBirb.Fonts.BMFont;
using BloomBirb.Graphics;
using BloomBirb.Graphics.Containers;
using BloomBirb.Graphics.Text;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.ResourceStores;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = BloomBirb.Renderers.OpenGL.Shader;

namespace BloomBirb
{
    public class Program
    {
        private static IWindow? window;
        private static OpenGlRenderer? gl;

        private static EmbeddedResourceStore? resources;

        private static ShaderStore? shaders;
        private static TextureStore? textures;
        private static AudioStore? audioStore;

        public static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "YOU COME INTO MY HOUSE, SUCK MY DICK, CALL ME GAY!";
            options.VSync = false;
            options.PreferredDepthBufferBits = 16;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.ForwardCompatible, new(4, 0));

            window = Window.Create(options);

            window.Load += onLoad;
            window.Render += onRender;
            window.Closing += onClose;
            window.Resize += onResize;

            window.Run();
        }

        private static AudioSource audioSource = null!;

        private static Container? container;

        private static Shader spriteShader;
        private static SpriteText? text;

        private static void onLoad()
        {
            IInputContext input = window?.CreateInput()!;
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += onKeyDown;
            }

            gl = new OpenGlRenderer();


            //Instantiating our new abstractions
            gl.Initialize(window!);

            resources = new EmbeddedResourceStore();
            textures = new TextureStore(gl, resources);
            shaders = new ShaderStore(gl, resources);
            audioStore = new AudioStore(resources);

            var fontTextures = new TextureStore(gl, resources, "Fonts");
            var fontStream = typeof(Font).Assembly.GetManifestResourceStream("BloomBirb.Resources.Fonts.OpenSans.fnt")!;
            var font = new Font(fontStream, fontTextures);

            spriteShader = shaders.Get("Texture", "Texture")!;

            spriteShader.Bind();

            spriteShader.SetUniform("u_Texture0", 0);

            text = new SpriteText(spriteShader, font)
            {
                Scale = new (0.5f),
                Colour = new(0,1,0.7f,1),
                RelativePositionAxes = Axes.Both,
                Position = new(0.05f)
            };

            Random rng = Random.Shared;

            container = new Container();
            onResize(window.Size);

            container.Add(new DrawableSprite(spriteShader)
            {
                Texture = textures.Get("sdfgh"),
                Size = new (0.6f),
                Anchor = Anchor.MiddleCentre,
                Origin = Anchor.MiddleCentre,
                RelativeSizeAxes = Axes.Both,
                Colour = new Vector4(1f,0,0.3f,1)

            });

            container.Add(text);

            gl.Context?.Enable(GLEnum.DepthTest);
            gl.Context?.DepthFunc(DepthFunction.Lequal);
            gl.Context?.Enable(GLEnum.Blend);
            gl.Context?.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);

            OpenAl.CreateContext();

            audioSource = new StreamedSoundSource(audioStore.Get("blue")!)
            {
                Volume = 0.25f,
                Speed = 1,
                Looping = true,
            };

            audioSource.Play();
        }

        private static float x = 0;

        private static int framesMade = 0;
        private static double timeElapsed = 0;
        private static void onRender(double obj)
        {
            gl?.BeginFrame();

            timeElapsed += obj;

            if(timeElapsed > 0.5)
            {
                text.Text = $"Frametime: {((timeElapsed * 1000) / framesMade):F5}";
                framesMade = 0;
                timeElapsed = 0;
            };
            framesMade++;

            x = (x + (float)obj * 30) % 360;

           // container.Rotation = x;
            //foreach (var child in container.Children)
                //child.Rotation += x/3000;

            //text.QueueDraw(gl!);
            container.QueueDraw(gl!);

            gl?.EndFrame();
        }

        private static void onResize(Vector2D<int> size)
        {
            gl?.Context?.Viewport(size);

            if(container != null)
            {
                container.Size = new(size.X, size.Y);
                container.Scale = new Vector2(2f / size.X, 2f / size.Y);
                container.Position = new Vector2(-1f);
            }
        }

        private static void onClose()
        {
            //Remember to dispose all the instances.
            audioSource?.Dispose();
            textures?.Dispose();
            shaders?.Dispose();
            gl?.Dispose();
        }

        private static void onKeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                window?.Close();
            }
        }

        private static string randomTexture(int i) => i switch
        {
            0 => "mike",
            1 => "sticky",
            2 => "kitty",
            3 => "funnyface",
            _ => "whatever"
        };
    }
}
