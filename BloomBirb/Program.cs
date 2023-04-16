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
        private static OpenGLRenderer? gl;

        private static EmbeddedResourceStore? resources;

        private static ShaderStore? shaders;
        private static TextureStore? textures;
        private static AudioStore? audioStore;

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "You spin me right round baby right round. Like a record baby right round round round.";
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

        private static Container container;

        private static Shader spriteShader;
        private static SpriteText? text;

        private static void onLoad()
        {
            IInputContext input = window?.CreateInput()!;
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += onKeyDown;
            }

            gl = new OpenGLRenderer();


            //Instantiating our new abstractions
            gl.Initialize(window!);

            resources = new EmbeddedResourceStore();
            textures = new TextureStore(gl, resources);
            shaders = new ShaderStore(gl, resources);
            audioStore = new AudioStore(resources);

            var fontTextures = new TextureStore(gl, resources, "Fonts");
            var fontStream = typeof(Font).Assembly.GetManifestResourceStream("BloomBirb.Resources.Fonts.OpenSans.fnt")!;
            var font = new Font(gl, fontStream, fontTextures);

            spriteShader = shaders.Get("Texture", "Texture")!;

            spriteShader.Bind();

            spriteShader.SetUniform("u_Texture0", 0);
            onResize(window.Size);

            text = new SpriteText(spriteShader, font);
            text.Text = "@";

            Random rng = Random.Shared;

            container = new();

            for (int i = 0; i < 10000; ++i)
            {
                var tex = textures.Get(randomTexture(rng.Next(5)))!;
                var sprite = new DrawableSprite(spriteShader)
                {
                    Texture = tex,
                    Position = new Vector2(rng.Next(1920), rng.Next(1080)),
                    Size = new Vector2(rng.Next(800), rng.Next(800)),
                    Scale = new Vector2(rng.NextSingle(), rng.NextSingle()),
                    Shear = new Vector2(rng.NextSingle(), rng.NextSingle()),
                    Rotation = rng.NextSingle() * 360,
                    Colour = new Vector4(rng.NextSingle(),rng.NextSingle(),rng.NextSingle(), 1)
                };
                container.Add(sprite);
            }

            container.Invalidate();

            gl.Context?.Enable(GLEnum.DepthTest);
            gl.Context?.DepthFunc(DepthFunction.Lequal);
            gl.Context?.Enable(GLEnum.Blend);
            gl.Context?.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);


            OpenAL.CreateContext();

            audioSource = new StreamedSoundSource(audioStore.Get("blue")!)
            {
                Volume = 0.25f,
                Speed = 1,
                Looping = true,
            };

            //audioSource.Play();
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
                text.Text = $"Frametime 拼: {((timeElapsed * 1000) / framesMade):F5}";
                framesMade = 0;
                timeElapsed = 0;
            };
            framesMade++;

            x = (x + (float)obj * 30) % 360;

            //container.Rotation = x;
            foreach (var child in container.Children)
                child.Rotation += x/3000;

            text.Invalidate();
            text.QueueDraw(gl!);

            container.Invalidate();
            container.QueueDraw(gl!);

            gl?.EndFrame();
        }

        private static void onResize(Vector2D<int> size)
        {
            gl?.Context?.Viewport(size);

            // HACK, IMPROVE LATER
            Matrix3 projectionMatrix = Matrix3.Identity;
            Matrix3Extensions.Translate(ref projectionMatrix, -1, -1);
            Matrix3Extensions.Scale(ref projectionMatrix, 1f / size.X, 1f / size.Y);

            spriteShader?.SetUniform("u_projMatrix", projectionMatrix);
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
