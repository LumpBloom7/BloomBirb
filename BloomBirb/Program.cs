using System.Numerics;
using BloomBirb.Audio;
using BloomBirb.Graphics;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Buffers;
using BloomBirb.ResourceStores;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

namespace BloomBirb
{
    public class Program
    {
        private static IWindow? window;
        private static GL? gl;

        //Our new abstracted objects, here we specify what the types are.
        private static QuadBuffer<TexturedVertex2D> quadBuffer = new QuadBuffer<TexturedVertex2D>(10000);

        private static EmbeddedResourceStore? resources = new();

        private static DrawableSprite[] sprites = new DrawableSprite[10000];

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "You spin me right round baby right round. Like a record baby right round round round.";
            options.VSync = false;
            options.API = new GraphicsAPI(ContextAPI.OpenGL, ContextProfile.Core, ContextFlags.Debug, new APIVersion(4, 5));
            window = Window.Create(options);

            window.Load += onLoad;
            window.Render += onRender;
            window.Closing += onClose;

            window.Run();
        }
        private static AudioSource audioSource = null!;
        private static void onLoad()
        {
            IInputContext input = window?.CreateInput()!;
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += onKeyDown;
            }

            gl = OpenGLRenderer.Initialize(window!);

            //Instantiating our new abstractions
            quadBuffer.Initialize(gl);
            var tex = resources?.Textures.Get("kitty")!;
            var shader = resources?.Shaders.Get("Texture", "Texture")!;

            Random rng = Random.Shared;
            for (int i = 0; i < sprites.Length; ++i)
            {
                var sprite = sprites[i] = new DrawableSprite(tex, shader);
                sprites[i].Position = new Vector2(rng.NextSingle() * 2 - 1, rng.NextSingle() * 2 - 1);
                sprites[i].Scale = new Vector2(rng.NextSingle() * 0.5f, rng.NextSingle() * 0.5f);
                sprites[i].Shear = new Vector2(rng.NextSingle(), rng.NextSingle());
                sprites[i].Rotation = rng.NextSingle() * 360;
                sprites[i].Colour = new Vector4(rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), rng.NextSingle());
                sprite.Invalidate();
            }


            OpenAL.CreateContext();

            audioSource = new StreamedSoundSource(resources?.Audio.Get("arrow")!)
            {
                Volume = 0.25f,
                Speed = 1,
                Looping = true,
            };

            //audioSource.Play();
        }

        private static unsafe void onRender(double obj)
        {
            gl?.Clear((uint)ClearBufferMask.ColorBufferBit);

            foreach (var sprite in sprites)
            {
                sprite.Rotation += (float)(obj * 180);
                sprite.Invalidate();
                sprite.Draw(gl!, quadBuffer);
            }

            quadBuffer.DrawBuffer();
        }

        private static void onClose()
        {
            //Remember to dispose all the instances.
            audioSource?.Dispose();
            quadBuffer.Dispose();
        }

        private static void onKeyDown(IKeyboard arg1, Key arg2, int arg3)
        {
            if (arg2 == Key.Escape)
            {
                window?.Close();
            }
        }
    }
}
