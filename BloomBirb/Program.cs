using System.Drawing;
using BloomBirb.Audio;
using BloomBirb.Extensions;
using BloomBirb.Graphics;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.ResourceStores;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenAL;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using Shader = BloomBirb.Renderers.OpenGL.Shader;
using Texture = BloomBirb.Renderers.OpenGL.Texture;

namespace BloomBirb
{
    public class Program
    {
        private static IWindow? window;
        private static GL? gl;

        //Our new abstracted objects, here we specify what the types are.
        private static BufferObject<float>? vbo;
        private static BufferObject<uint>? ebo;
        private static VertexArrayObject<float, uint>? vao;

        // Note to self Screen origin is bottom left, (0,0) is centre
        // UV origin is topleft.
        private static readonly float[] vertices =
        {
            //X    Y      Z     U   V
            -0.5f, -0.5f , 1, 0, 1,
             0.5f, -0.5f , 1, 1, 1,
            -0.5f,  0.5f , 1, 0, 0,
             0.5f,  0.5f , 1, 1, 0
        };

        private static readonly uint[] indices =
        {
            0, 1, 2,
            1, 3, 2
        };

        private static EmbeddedResourceStore? resources = new();

        private static DrawableSprite sprite = null!;

        private static void Main(string[] args)
        {
            var options = WindowOptions.Default;
            options.Size = new Vector2D<int>(1024, 768);
            options.Title = "You spin me right round baby right round. Like a record baby right round round round.";
            window = Window.Create(options);

            window.Load += onLoad;
            window.Render += onRender;
            window.Closing += onClose;

            window.Run();
        }
        private static StreamedSoundSource audioSource;
        private static void onLoad()
        {
            IInputContext input = window?.CreateInput()!;
            for (int i = 0; i < input.Keyboards.Count; i++)
            {
                input.Keyboards[i].KeyDown += onKeyDown;
            }

            gl = OpenGLRenderer.CreateContext(window!);

            //Instantiating our new abstractions
            ebo = new BufferObject<uint>(indices, BufferTargetARB.ElementArrayBuffer);
            vbo = new BufferObject<float>(vertices, BufferTargetARB.ArrayBuffer);
            vao = new VertexArrayObject<float, uint>(vbo, ebo);

            //Telling the VAO object how to lay out the attribute pointers
            vao.VertexAttributePointer(0, 3, VertexAttribPointerType.Float, 5, 0);
            vao.VertexAttributePointer(1, 2, VertexAttribPointerType.Float, 5, 3);

            vao?.Bind();

            sprite = new DrawableSprite(resources?.Textures.Get("kitty")!, resources?.Shaders.Get("Texture", "Texture")!);

            OpenAL.CreateContext();

            audioSource = new StreamedSoundSource(new MP3Audio(resources?.Get("Audio.arrow.mp3")!))
            {
                Volume = 0.25f,
                Looping = true,
            };

            audioSource.Play();
        }

        private static float speed = 1.0f;
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

            gl?.DrawElements(PrimitiveType.Triangles, (uint)indices.Length, DrawElementsType.UnsignedInt, null);
        }

        private static void onClose()
        {
            //Remember to dispose all the instances.
            vbo?.Dispose();
            ebo?.Dispose();
            vao?.Dispose();
            audioSource?.Dispose();
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
