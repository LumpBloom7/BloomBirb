using System.Numerics;
using BloomBirb.Audio;
using BloomBirb.Graphics;
using BloomBirb.Graphics.Vertices;
using BloomBirb.Renderers.OpenGL;
using BloomBirb.Renderers.OpenGL.Batches;
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
        private static OpenGLRenderer? gl;

        //Our new abstracted objects, here we specify what the types are.
        private static QuadBatch<DepthWrappingVertex<TexturedVertex2D>>? quadBuffer;

        private static EmbeddedResourceStore? resources;

        private static ShaderStore? shaders;
        private static TextureStore? textures;
        private static AudioStore? audioStore;

        private static DrawableSprite[] sprites = new DrawableSprite[10000];

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

            gl = new OpenGLRenderer();

            quadBuffer = new QuadBatch<DepthWrappingVertex<TexturedVertex2D>>(gl, 10000, 100);
            resources = new EmbeddedResourceStore();
            textures = new TextureStore(gl, resources);
            shaders = new ShaderStore(gl, resources);
            audioStore = new AudioStore(resources);

            //Instantiating our new abstractions
            gl.Initialize(window!);
            quadBuffer.Initialize();

            var tex = textures.Get("sticky")!;
            var shader = shaders.Get("Texture", "Texture")!;

            shader.Bind();

            shader.SetUniform("u_Texture0", 0);

            Random rng = Random.Shared;
            for (int i = 0; i < sprites.Length; ++i)
            {
                var sprite = sprites[i] = new DrawableSprite(tex, shader);
                sprites[i].Position = new Vector2(rng.NextSingle() * 2 - 1, rng.NextSingle() * 2 - 1);
                sprites[i].Scale = new Vector2(rng.NextSingle() * 0.5f, rng.NextSingle() * 0.5f);
                sprites[i].Shear = new Vector2(rng.NextSingle(), rng.NextSingle());
                sprites[i].Rotation = rng.NextSingle() * 360;
                sprites[i].Colour = new Vector4(rng.NextSingle(), rng.NextSingle(), rng.NextSingle(), 1f);
                sprite.Invalidate();
            }

            gl.Context?.Enable(GLEnum.Blend);
            gl.Context?.BlendFunc(GLEnum.SrcAlpha, GLEnum.OneMinusSrcAlpha);
            gl.Context?.Enable(GLEnum.DepthTest);
            gl.Context?.DepthFunc(DepthFunction.Lequal);


            OpenAL.CreateContext();

            audioSource = new StreamedSoundSource(audioStore.Get("blue")!)
            {
                Volume = 0.25f,
                Speed = 1,
                Looping = true,
            };

            //audioSource.Play();
        }

        private static void onRender(double obj)
        {
            gl?.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            foreach (var sprite in sprites)
            {
                sprite.Rotation += (float)(obj * 180);
                sprite.Invalidate();
                sprite.Draw(gl!, quadBuffer!);
                DepthWrappingVertex<TexturedVertex2D>.Increment();
            }

            quadBuffer?.FlushBatch();

            DepthWrappingVertex<TexturedVertex2D>.Reset();
        }

        private static void onClose()
        {
            //Remember to dispose all the instances.
            audioSource?.Dispose();
            quadBuffer?.Dispose();
            gl?.Dispose();
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
