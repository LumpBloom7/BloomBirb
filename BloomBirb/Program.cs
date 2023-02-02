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
        private static QuadBuffer<TexturedVertex2D> quadBuffer = new QuadBuffer<TexturedVertex2D>(1);

        // Note to self Screen origin is bottom left, (0,0) is centre
        // UV origin is topleft.
        private static readonly TexturedVertex2D[] vertices =
        {
            //X    Y       U  V
            new(-0.5f, 0.5f , 0, 1),
            new (-0.5f, -0.5f , 0, 0),
            new(0.5f,  -0.5f , 1, 0),
            new( 0.5f,  0.5f , 1, 1)
        };

        private static EmbeddedResourceStore? resources = new();

        private static DrawableSprite sprite = null!;

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "You spin me right round baby right round. Like a record baby right round round round.";
            options.VSync = false;
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

            gl = OpenGLRenderer.CreateContext(window!);

            //Instantiating our new abstractions
            quadBuffer.Initialize(gl);
            quadBuffer.BufferData(vertices, 0);

            sprite = new DrawableSprite(resources?.Textures.Get("kitty")!, resources?.Shaders.Get("Texture", "Texture")!);

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

            float time = ((DateTime.Now.Second % 5) + DateTime.Now.Millisecond / 1000f) / 5f;
            float timeRot = (DateTime.Now.Second + DateTime.Now.Millisecond / 1000f) / 60f;

            // Everything is a sinewave
            sprite.Rotation = MathF.Sin(timeRot * 6.28f) * 360;
            float shear = MathF.Sin(time * 6.28f + 1) * 0.25f;
            float shearY = MathF.Sin(time * 6.28f + 2) * 0.25f;
            float sclX = MathF.Sin(time * 6.28f + 3) * 1;
            float sclY = MathF.Sin(time * 6.28f + 4) * 1;
            float trnsX = MathF.Sin(time * 6.28f + 5) * 0.5f;
            float trnsY = MathF.Sin(time * 6.28f + 6) * 0.5f;

            float r = MathF.Abs(MathF.Sin(time * 6.28f) * 1f);
            float g = MathF.Abs(MathF.Sin(time * 6.28f + 2) * 1f);
            float b = MathF.Abs(MathF.Sin(time * 6.28f + 4) * 1f);

            sprite.Scale = new System.Numerics.Vector2(sclX, sclY);
            sprite.Position = new System.Numerics.Vector2(trnsX, trnsY);
            sprite.Shear = new System.Numerics.Vector2(shear, shearY);
            sprite.Colour = new System.Numerics.Vector4(r, g, b, 1f);

            sprite.Invalidate();
            sprite.Draw(gl!);

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
